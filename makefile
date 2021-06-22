# Version checking
MSBUILD := msbuild
MIN_MONO_VERSION:= "5.18.0"
MONO_VERSION:= $(shell mono --version | awk '/version/ { print $$5 }')
MIN_NUGET_VERSION:= "2.16.0"
NUGET_VERSION:= $(shell nuget help 2> /dev/null | awk '/Version:/ { print $$3; exit 0}')
GreaterVersion = $(shell printf '%s\n' $(1) $(2) | sort -t. -k 1,1nr -k 2,2nr -k 3,3nr -k 4,4nr | head -n 1)
PROGRAM_VERSION = $(shell git describe --tags --exact-match 2> /dev/null)

# Directories
DEBUG_DIR   := bin_debug
RELEASE_DIR := bin_release
OUTPUT_DIR  := $(DEBUG_DIR)

# Final output names
MAC_BUILD_RESULT = macbuild.dmg
LINUX_BUILD_RESULT = linuxbuild.zip
DEBIAN_BUILD_RESULT = debianbuild.deb

# Used output name
ifeq ($(shell uname -s),Darwin) 
BUILD_RESULT = $(MAC_BUILD_RESULT)
else
BUILD_RESULT = $(LINUX_BUILD_RESULT)
endif

# Colors
COLOR_BLACK   := "\033[1;30m"
COLOR_RED     := "\033[1;31m"
COLOR_GREEN   := "\033[1;32m"
COLOR_YELLOW  := "\033[1;33m"
COLOR_BLUE    := "\033[1;34m"
COLOR_MAGENTA := "\033[1;35m"
COLOR_CYAN    := "\033[1;36m"
COLOR_WHITE   := "\033[1;37m"
COLOR_END     := "\033[0m"
#Literal sequences don't work in the info command....
red:=$(shell tput setaf 1)
green:=$(shell tput setaf 2)
blue:=$(shell tput setaf 4)
reset:=$(shell tput sgr0)

.PHONY: all 
.PHONY: all-debug
.PHONY: all-release
.PHONY: debug
.PHONY: release 
.PHONY: clean
.PHONY: clean-all
.PHONY: openbve
.PHONY: openbve-debug
.PHONY: openbve-release
.PHONY: publish
.PHONY: debian
.PHONY: restore
.PHONY: prequisite-check

restore:
	nuget restore OpenBVE.sln

debug: openbve-debug
release: openbve-release
openbve: openbve-debug

openbve-debug: restore
	$(info Building OpenBVE in debug mode....)
	$(MSBUILD) /t:OpenBve /p:Configuration=Debug OpenBVE.sln

openbve-release: restore
	$(info Building OpenBVE in release mode....)
	$(MSBUILD) /t:OpenBve /p:Configuration=Release OpenBVE.sln

all: all-debug

all-debug: restore
	$(info Building OpenBVE and developer tools in debug mode....)
	$(MSBUILD) /t:build /p:Configuration=Debug OpenBVE.sln

all-release: restore
	$(info Building OpenBVE and developer tools in release mode....)
	$(MSBUILD) /t:build /p:Configuration=Release OpenBVE.sln

clean-all: clean

clean:
	$(info Runing solution clean....)
	$(MSBUILD) /t:clean /p:Configuration=Debug OpenBVE.sln
	$(MSBUILD) /t:clean /p:Configuration=Release OpenBVE.sln

	# Release Files
	rm -f $(MAC_BUILD_RESULT) $(LINUX_BUILD_RESULT)

CP_UPDATE_FLAG = -u
CP_RECURSE = -r
ifeq ($(shell uname -s),Darwin) 
    CP_UPDATE_FLAG = 
    CP_RECURSE = -R
endif 

ifeq ($(shell uname -s),Darwin) 
publish: $(MAC_BUILD_RESULT)
else
publish: $(LINUX_BUILD_RESULT)
endif

debian: $(DEBIAN_BUILD_RESULT)

prequisite-check:
 #Very basic prequisite check function
 $(info Checking for prequisite system libraries.)
 $(info Checking for Mono....)
 ifeq (, $(shell which mono))
 $(info Mono does not appear to be installed on this system.)
 $(info Please install the $(green)mono-complete$(reset) package provided by your distribution, or the latest version of Mono from $(blue)https://www.mono-project.com/$(reset))
 $(error )
 endif
 $(info Mono Version $(MONO_VERSION) found.)
 ifeq "$(call GreaterVersion, $(MONO_VERSION), $(MIN_MONO_VERSION))" "$(MONO_VERSION)"
 #Nothing
 else
 $(info OpenBVE requires a minimum Mono version of 5.20)
 $(info Please install the latest version of Mono from $(blue)https://www.mono-project.com/$(reset))
 $(error )
 endif
 $(info Checking for MSBuild....)
 ifeq (, $(shell which msbuild))
 $(info msbuild does not appear to be installed on this system.)
 $(info Please either install the $(green)mono-xbuild$(reset) package provided by your distribution, or the latest version of Mono from $(blue)https://www.mono-project.com/$(reset))
 $(error )
 endif
 $(info Checking for nuget....)
 ifeq (, $(shell which nuget))
 $(info nuget does not appear to be installed on this system.)
 $(info Please either install the $(green)nuget$(reset) package provided by your distribution, or the latest version of Mono from $(blue)https://www.mono-project.com/$(reset))
 $(error )
 endif
 $(info nuget Version $(NUGET_VERSION) found.)
 ifeq "$(call GreaterVersion, $(NUGET_VERSION), $(MIN_NUGET_VERSION))" "$(NUGET_VERSION)"
 #Nothing
 else
 $(info OpenBVE requires a minimum nuget version of 2.16)
 $(info Please run $(red)nuget update -self$(reset) with administrative priveledges.)
 $(error )
 endif
 $(info Attempting to restore nuget packages (This may take a while)....)
	
$(MAC_BUILD_RESULT): all-release
	@rm -rf bin_release/DevTools/
	@echo $(COLOR_RED)Decompressing $(COLOR_CYAN)installers/mac/MacBundle.tgz$(COLOR_END)
	# Clear previous Mac build temporary files if they exist
	@rm -rf mac
	@rm -rf macbuild.dmg
	@mkdir mac
	@tar -C mac -xzf installers/mac/MacBundle.tgz

	@echo $(COLOR_RED)Copying build data into $(COLOR_CYAN)OpenBVE.app$(COLOR_END)
	@cp -r $(RELEASE_DIR)/* mac/OpenBVE.app/Contents/Resources/

	@echo $(COLOR_RED)Creating $(COLOR_CYAN)$(MAC_BUILD_RESULT)$(COLOR_END)
	@hdiutil create $(MAC_BUILD_RESULT) -volname "OpenBVE" -fs HFS+ -srcfolder "mac/OpenBVE.app"
	@echo Renaming final output file
ifeq (, $(PROGRAM_VERSION))
	@echo This is a $(COLOR_BLUE)Daily build$(COLOR_END)
	@echo Final filename: $(COLOR_RED)OpenBVE-$$(date '+%F').dmg$(COLOR_END)
	@mv macbuild.dmg OpenBVE-$$(date '+%F').dmg
else
	@echo This is a $(COLOR_YELLOW)Tagged Release build$(COLOR_END)
	@echo Final filename: $(COLOR_RED)OpenBVE-$(PROGRAM_VERSION).dmg$(COLOR_END)
	@mv macbuild.dmg OpenBVE-$(PROGRAM_VERSION).dmg
endif

$(LINUX_BUILD_RESULT): all-release
	@rm -rf bin_release/DevTools/
	@echo $(COLOR_RED)Compressing $(COLOR_CYAN)$(LINUX_BUILD_RESULT)$(COLOR_END)
	@cd $(RELEASE_DIR); zip -qr9Z deflate ../$(LINUX_BUILD_RESULT) *
	@echo Renaming final output file
ifeq (, $(PROGRAM_VERSION))
	@echo This is a $(COLOR_BLUE)Daily build$(COLOR_END)
	@echo Final filename: $(COLOR_RED)OpenBVE-$$(date '+%F').zip$(COLOR_END)
	@mv linuxbuild.zip OpenBVE-$$(date '+%F').zip
else
	@echo This is a $(COLOR_YELLOW)Tagged Release build$(COLOR_END)
	@echo Final filename: $(COLOR_RED)OpenBVE-$(PROGRAM_VERSION).zip$(COLOR_END)
	@mv linuxbuild.zip OpenBVE-$(PROGRAM_VERSION).zip
endif

$(DEBIAN_BUILD_RESULT): all-release
	@rm -rf bin_release/DevTools/
	@echo $(COLOR_RED)Copying build into place....$(COLOR_END)
	@mkdir -p installers/debian/usr/lib/openbve
#Generate current dpkg control file
	@./DebianControl.sh
#Mark launch script as executable before packaging
#Also deliberately chmod assets directory to 755- https://github.com/leezer3/OpenBVE/issues/656#issuecomment-865164917
	@chmod -R 755 bin_release/Data
	@chmod +x installers/debian/usr/games/openbve
	@cp -r -f $(CP_UPDATE_FLAG) $(RELEASE_DIR)/* installers/debian/usr/lib/openbve
	@echo $(COLOR_RED)Compressing $(COLOR_CYAN)$(DEBIAN_BUILD_RESULT)$(COLOR_END)
	@fakeroot dpkg-deb --build installers/debian
	@echo Renaming final output file
ifeq (, $(PROGRAM_VERSION))
	@echo This is a $(COLOR_BLUE)Daily build$(COLOR_END)
	@echo Final filename: $(COLOR_RED)OpenBVE-$$(date '+%F').deb$(COLOR_END)
	@mv installers/debian.deb OpenBVE-$$(date '+%F').deb
else
	@echo This is a $(COLOR_YELLOW)Tagged Release build$(COLOR_END)
	@echo Final filename: $(COLOR_RED)OpenBVE-$$(date '+%F').deb$(COLOR_END)
	@mv installers/debian.deb OpenBVE-$(PROGRAM_VERSION).deb
endif