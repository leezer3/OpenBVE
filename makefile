# C-Sharp Compiler
HAS_MSBUILD := $(shell command -v msbuild 2> /dev/null)

ifdef HAS_MSBUILD
	MSBUILD := msbuild
endif

ifndef HAS_MSBUILD
	MSBUILD := xbuild
endif

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

restore:
	nuget restore OpenBVE.sln

debug: openbve-debug
release: openbve-release
openbve: openbve-debug

openbve-debug: restore
	$(MSBUILD) /t:OpenBve /p:Configuration=Debug OpenBVE.sln

openbve-release: restore
	$(MSBUILD) /t:OpenBve /p:Configuration=Release OpenBVE.sln

all: all-debug

all-debug: restore
	$(MSBUILD) /t:build /p:Configuration=Debug OpenBVE.sln

all-release: restore
	$(MSBUILD) /t:build /p:Configuration=Release OpenBVE.sln

clean-all: clean

clean:
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

$(LINUX_BUILD_RESULT): all-release
	@rm -rf bin_release/DevTools/
	@echo $(COLOR_RED)Compressing $(COLOR_CYAN)$(LINUX_BUILD_RESULT)$(COLOR_END)
	@cd $(RELEASE_DIR); zip -qr9Z deflate ../$(LINUX_BUILD_RESULT) *

$(DEBIAN_BUILD_RESULT): all-release
	@rm -rf bin_release/DevTools/
	@echo $(COLOR_RED)Copying build into place....$(COLOR_END)
	@mkdir -p installers/debian/usr/lib/openbve
#Generate current dpkg control file
	@./DebianControl.sh
#Mark launch script as executable before packaging
	@chmod +x installers/debian/usr/games/openbve
	@cp -r -f $(CP_UPDATE_FLAG) $(RELEASE_DIR)/* installers/debian/usr/lib/openbve
	@echo $(COLOR_RED)Compressing $(COLOR_CYAN)$(DEBIAN_BUILD_RESULT)$(COLOR_END)
	@fakeroot dpkg-deb --build installers/debian
