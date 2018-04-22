# C-Sharp Compiler
HAS_GMCS := $(shell command -v gmcs 2> /dev/null)
HAS_SMCS := $(shell command -v smcs 2> /dev/null)

# This is so hacky it's not even funny
# Should work though. That's the scary part.
# This horrifies me.
ifdef HAS_GMCS
	CSC := gmcs -pkg:dotnet -lib:/usr/lib/mono/4.0
	CSC_NAME :=gmcs
endif

ifndef HAS_GMCS
	ifdef HAS_SMCS
		CSC := smcs -pkg:dotnet -lib:/usr/lib/mono/4.0
		CSC_NAME :=smcs
	endif
endif

ifndef HAS_GMCS
	ifndef HAS_SMCS
			CSC := mcs
			CSC_NAME :=mcs
	endif
endif

# Resource file creator
RESGEN := resgen

# Standard Arguments
DEBUG_ARGS   := /noconfig /debug:Full /debug+ /optimize- /warnaserror- /unsafe+ /define:"DEBUG;TRACE" /platform:x86 /warn:4 /pkg:dotnet
RELEASE_ARGS := /noconfig /debug- /optimize+ /unsafe+ /checked- /define:"TRACE" /platform:x86 /warn:4 /pkg:dotnet

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

# Current Args
ARGS := $(DEBUG_ARGS)

# Thumbnail
ICON := assets/icon.ico

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

#######################
# Project Information #
#######################

# This has to be forward declared for it to work right
OPEN_BVE_ROOT         :=source/OpenBVE
OPEN_BVE_FILE         :=OpenBve.exe

OPEN_BVE_API_ROOT     :=source/OpenBveApi
OPEN_BVE_API_FILE     :=OpenBveApi.dll

OPEN_BVE_ATS_ROOT     :=source/Plugins/OpenBveAts
OPEN_BVE_ATS_FILE     :=Data/Plugins/OpenBveAts.dll

SOUND_FLAC_ROOT       :=source/Plugins/Sound.Flac
SOUND_FLAC_FILE       :=Data/Plugins/Sound.Flac.dll

SOUND_RIFFWAVE_ROOT   :=source/Plugins/Sound.RiffWave
SOUND_RIFFWAVE_FILE   :=Data/Plugins/Sound.RiffWave.dll

TEXTURE_ACE_ROOT      :=source/Plugins/Texture.Ace
TEXTURE_ACE_FILE      :=Data/Plugins/Texture.Ace.dll

TEXTURE_BGJPT_ROOT    :=source/Plugins/Texture.BmpGifJpegPngTiff
TEXTURE_BGJPT_FILE    :=Data/Plugins/Texture.BmpGifJpegPngTiff.dll

TEXTURE_DDS_ROOT      :=source/Plugins/Texture.Dds
TEXTURE_DDS_FILE      :=Data/Plugins/Texture.Dds.dll

ROUTE_VIEWER_ROOT     :=source/RouteViewer
ROUTE_VIEWER_FILE     :=RouteViewer.exe

OBJECT_BENDER_ROOT    :=source/ObjectBender
OBJECT_BENDER_FILE    :=ObjectBender.exe

CAR_XML_ROOT    :=source/CarXMLConvertor
CAR_XML_FILE    :=CarXMLConvertor.exe

OBJECT_VIEWER_ROOT    :=source/ObjectViewer
OBJECT_VIEWER_FILE    :=ObjectViewer.exe

TRAIN_EDITOR_ROOT     :=source/TrainEditor
TRAIN_EDITOR_FILE     :=TrainEditor.exe

LBAHEADER_ROOT        :=source/DevTools/LBAHeader
LBAHEADER_FILE        :=DevTools/LBAHeader.exe

# Dependences 
DEBUG_DEPEND := $(patsubst dependencies/%,$(DEBUG_DIR)/%,$(wildcard dependencies/*))
RELEASE_DEPEND := $(patsubst dependencies/%,$(RELEASE_DIR)/%,$(wildcard dependencies/*))
DEBUG_ASSETS := $(patsubst assets/%,$(DEBUG_DIR)/Data/%,$(wildcard assets/*))
RELEASE_ASSETS := $(patsubst assets/%,$(DEBUG_DIR)/Data/%,$(wildcard assets/*))

.PHONY: all 
.PHONY: debug
.PHONY: release 
.PHONY: clean
.PHONY: clean-all
.PHONY: openbve
.PHONY: openbve-debug
.PHONY: openbve-release
.PHONY: all
.PHONY: all-debug
.PHONY: all-release
.PHONY: prep_dirs
.PHONY: prep_release_dirs
.PHONY: copy_depends
.PHONY: copy_release_depends
.PHONY: publish
.PHONY: debian
.PHONY: print_csc_type

debug: openbve-debug
release: openbve-release
openbve: openbve-debug

openbve-debug: print_csc_type
openbve-debug: $(DEBUG_DIR)/$(OPEN_BVE_FILE)
openbve-debug: copy_depends

openbve-release: print_csc_type
openbve-release: ARGS := $(RELEASE_ARGS)
openbve-release: OUTPUT_DIR := $(RELEASE_DIR)
openbve-release: $(RELEASE_DIR)/$(OPEN_BVE_FILE)
openbve-release: copy_release_depends

all: all-debug

all-debug: print_csc_type
all-debug: $(DEBUG_DIR)/$(OPEN_BVE_FILE)
all-debug: $(DEBUG_DIR)/$(OBJECT_BENDER_FILE)
all-debug: $(DEBUG_DIR)/$(CAR_XML_FILE)
all-debug: $(DEBUG_DIR)/$(OBJECT_VIEWER_FILE)
all-debug: $(DEBUG_DIR)/$(ROUTE_VIEWER_FILE)
all-debug: $(DEBUG_DIR)/$(TRAIN_EDITOR_FILE)
all-debug: $(DEBUG_DIR)/$(LBAHEADER_FILE)
all-debug: copy_depends

all-release: print_csc_type
all-release: ARGS := $(RELEASE_ARGS)
all-release: OUTPUT_DIR := $(RELEASE_DIR)
all-release: $(RELEASE_DIR)/$(OPEN_BVE_FILE)
all-release: $(RELEASE_DIR)/$(OBJECT_BENDER_FILE)
all-release: $(RELEASE_DIR)/$(CAR_XML_FILE)
all-release: $(RELEASE_DIR)/$(OBJECT_VIEWER_FILE)
all-release: $(RELEASE_DIR)/$(ROUTE_VIEWER_FILE)
all-release: $(RELEASE_DIR)/$(TRAIN_EDITOR_FILE)
all-release: $(RELEASE_DIR)/$(LBAHEADER_FILE)
all-release: copy_release_depends

CP_UPDATE_FLAG = -u
CP_RECURSE = -r
ifeq ($(shell uname -s),Darwin) 
    CP_UPDATE_FLAG = 
    CP_RECURSE = -R
endif 

print_csc_type:
	@echo $(COLOR_RED)Using $(CSC_NAME) as c\# compiler$(COLOR_END)

$(DEBUG_DEPEND): $(patsubst $(DEBUG_DIR)/%,dependencies/%,$@) | $(DEBUG_DIR) $(DEBUG_DIR)/Data/Plugins $(DEBUG_DIR)/DevTools
$(RELEASE_DEPEND): $(patsubst $(RELEASE_DIR)/%,dependencies/%,$@) | $(RELEASE_DIR) $(RELEASE_DIR)/Data/Plugins $(RELEASE_DIR)/DevTools

$(DEBUG_DEPEND) $(RELEASE_DEPEND):
	@echo $(COLOR_BLUE)Copying dependency $(COLOR_CYAN)$@$(COLOR_END)
	@cp -r $(CP_UPDATE_FLAG) $(patsubst $(OUTPUT_DIR)/%,dependencies/%,$@) $(OUTPUT_DIR)/

$(DEBUG_DIR) $(RELEASE_DIR): 
	@echo $(COLOR_BLUE)Creating directory $(COLOR_CYAN)$(OUTPUT_DIR)$(COLOR_END)
	@mkdir -p $(OUTPUT_DIR)

$(DEBUG_DIR)/DevTools $(RELEASE_DIR)/DevTools: 
	@echo $(COLOR_BLUE)Creating directory $(COLOR_CYAN)$(OUTPUT_DIR)/DevTools$(COLOR_END)
	@mkdir -p $(OUTPUT_DIR)/DevTools

$(DEBUG_DIR)/Data $(RELEASE_DIR)/Data:
	@echo $(COLOR_BLUE)Creating directory $(COLOR_CYAN)$(OUTPUT_DIR)/Data/$(COLOR_END)
	@mkdir -p $(OUTPUT_DIR)/Data/

$(DEBUG_DIR)/Data/Plugins $(RELEASE_DIR)/Data/Plugins:
	@echo $(COLOR_BLUE)Creating directory $(COLOR_CYAN)$(OUTPUT_DIR)/Data/Plugins$(COLOR_END)
	@mkdir -p $(OUTPUT_DIR)/Data/Plugins/

copy_depends: $(DEBUG_DIR)/Data
copy_release_depends: $(RELEASE_DIR)/Data

copy_depends copy_release_depends:
	@echo $(COLOR_BLUE)Copying $(COLOR_CYAN)assets/*$(COLOR_BLUE) to $(COLOR_CYAN)$(OUTPUT_DIR)/Data/*$(COLOR_END)
	@cp -r $(CP_UPDATE_FLAG) assets/* $(OUTPUT_DIR)/Data

clean: 
	# Executables
	rm -f bin*/ObjectBender.exe* bin*/ObjectBender.pdb
	rm -f bin*/ObjectViewer.exe* bin*/ObjectViewer.pdb
	rm -f bin*/OpenBve.exe* bin*/OpenBve.pdb
	rm -f bin*/OpenBveObjectValidator.exe* bin*/OpenBveObjectValidator.pdb
	rm -f bin*/RouteViewer.exe* bin*/RouteViewer.pdb
	rm -f bin*/TrainEditor.exe* bin*/TrainEditor.pdb

	# DLL
	rm -f bin*/OpenBveApi.dll* bin*/OpenBveApi.pdb
	rm -f bin*/Data/Plugins/OpenBveAts.dll* bin*/Data/Plugins/OpenBveAts.pdb
	rm -f bin*/Data/Plugins/Sound.Flac.dll* bin*/Data/Plugins/Sound.Flac.pdb
	rm -f bin*/Data/Plugins/Sound.RiffWave.dll* bin*/Data/Plugins/Sound.RiffWave.pdb
	rm -f bin*/Data/Plugins/Texture.Ace.dll* bin*/Data/Plugins/Texture.Ace.pdb
	rm -f bin*/Data/Plugins/Texture.BmpGifJpegPngTiff.dll* bin*/Data/Plugins/Texture.BmpGifJpegPngTiff.pdb
	rm -f bin*/Data/Plugins/Texture.Dds.dll* bin*/Data/Plugins/Texture.Dds.pdb

	# Release Files
	rm -f $(MAC_BUILD_RESULT) $(LINUX_BUILD_RESULT)

	# Resource Files
	rm -f `find . | grep .resources | tr '\n' ' '`

	# Assembly
	rm -f $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs

clean-all: clean
	# DevTools
	rm -f bin*/DevTools/LBAHeader.exe* bin*/DevTools/LBAHeader.pdb

	# Everything else
	rm -rf bin*/

ifeq ($(shell uname -s),Darwin) 
publish: $(MAC_BUILD_RESULT)
else
publish: $(LINUX_BUILD_RESULT)
endif

debian: $(DEBIAN_BUILD_RESULT)

$(MAC_BUILD_RESULT): all-release
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
	@echo $(COLOR_RED)Compressing $(COLOR_CYAN)$(LINUX_BUILD_RESULT)$(COLOR_END)
	@cd $(RELEASE_DIR); zip -qr9Z deflate ../$(LINUX_BUILD_RESULT) *

$(DEBIAN_BUILD_RESULT): all-release
	@echo $(COLOR_RED)Copying build into place....$(COLOR_END)
	@mkdir -p installers/debian/usr/lib/openbve
#Generate current dpkg control file
	@./DebianControl.sh
#Mark launch script as executable before packaging
	@chmod +x installers/debian/usr/games/openbve
	@cp -r -f $(CP_UPDATE_FLAG) $(RELEASE_DIR)/* installers/debian/usr/lib/openbve
	@echo $(COLOR_RED)Compressing $(COLOR_CYAN)$(DEBIAN_BUILD_RESULT)$(COLOR_END)
	@fakeroot dpkg-deb --build installers/debian

# Utility target generator that allows easier generation of resource files

define resource_rule_impl
$1: $2 
	@echo $$(COLOR_GREEN)Generating resource file $$(COLOR_CYAN)$$@$(COLOR_END)
	@$(RESGEN) /useSourcePath /compile "$$<,$$@" > /dev/null
endef

create_resource = $(foreach combo, $(join $(1), $(foreach resx, $(2), $(addprefix ^, $(resx)))), $(call create_resource_tmp, $(combo)))
create_resource_tmp = $(eval $(call resource_rule_impl, $(firstword $(subst ^, ,$(1))), $(word 2, $(subst ^, ,$(1)))))

################################
########### Projects ###########
################################

###########
# OpenBve #
###########

OPEN_BVE_FOLDERS  := . Audio Game Game/AI Game/Events Game/Events/EventTypes Game/ObjectManager Game/ObjectManager/AnimatedObjects Game/Score Game/TrackManager Graphics Graphics/Renderer Interface OldCode Parsers Parsers/Object/BVE Parsers/Object/Generic Parsers/Object/Loksim3D Parsers/Panel Parsers/Routes Parsers/Route/BVE Parsers/Script Parsers/SoundConfiguration Parsers/Train Properties OldParsers OldParsers/BveRouteParser Simulation/TrainManager Simulation/TrainManager/Car Simulation/TrainManager/Motor Simulation/TrainManager/Train Simulation/World System System/Functions System/Input System/Logging System/Plugins System/Program System/Translations UserInterface
OPEN_BVE_FOLDERS  := $(addprefix $(OPEN_BVE_ROOT)/, $(OPEN_BVE_FOLDERS))
OPEN_BVE_SRC      := $(filter-out "$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs",$(patsubst %, "%", $(foreach sdir, $(OPEN_BVE_FOLDERS), $(wildcard $(sdir)/*.cs))))
OPEN_BVE_DOC      := $(addprefix /doc:, $(foreach sdir, $(OPEN_BVE_FOLDERS), $(wildcard $(sdir)/*.xml)))
OPEN_BVE_RESX     := $(foreach sdir, $(OPEN_BVE_FOLDERS), $(wildcard $(sdir)/*.resx))
OPEN_BVE_RESOURCE := $(subst Properties.,,$(subst OldCode.,,$(subst UserInterface.,,$(addprefix $(OPEN_BVE_ROOT)/, $(subst /,., $(subst /./,/,  $(patsubst $(dir $(OPEN_BVE_ROOT))%.resx, %.resources, $(OPEN_BVE_RESX))))))))
OPEN_BVE_OUT       =$(OUTPUT_DIR)/$(OPEN_BVE_FILE)

$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs: $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.sh
	@echo $(COLOR_WHITE)Creating assembly file $(COLOR_CYAN)$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs$(COLOR_END)
	@bash $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.sh

$(call create_resource, $(OPEN_BVE_RESOURCE), $(OPEN_BVE_RESX))

$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE) 
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(OPEN_BVE_ATS_FILE) 
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(SOUND_FLAC_FILE) 
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(SOUND_RIFFWAVE_FILE) 
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(TEXTURE_ACE_FILE) 
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(TEXTURE_BGJPT_FILE)
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(TEXTURE_DDS_FILE) 
$(DEBUG_DIR)/$(OPEN_BVE_FILE): $(DEBUG_DIR)/$(LBAHEADER_FILE)

$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE) 
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(OPEN_BVE_ATS_FILE) 
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(SOUND_FLAC_FILE) 
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(SOUND_RIFFWAVE_FILE) 
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(TEXTURE_ACE_FILE) 
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(TEXTURE_BGJPT_FILE)
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(TEXTURE_DDS_FILE) 
$(RELEASE_DIR)/$(OPEN_BVE_FILE): $(RELEASE_DIR)/$(LBAHEADER_FILE)

$(DEBUG_DIR)/$(OPEN_BVE_FILE) $(RELEASE_DIR)/$(OPEN_BVE_FILE): $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs $(patsubst "%", %, $(OPEN_BVE_SRC)) $(OPEN_BVE_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OPEN_BVE_OUT)$(COLOR_END)
	@$(CSC) /out:$(OPEN_BVE_OUT) /target:winexe /main:OpenBve.Program $(OPEN_BVE_SRC) $(ARGS) $(OPEN_BVE_DOC) \
	$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs \
	/reference:$(OUTPUT_DIR)/OpenTK.dll /reference:$(OPEN_BVE_API_OUT) \
	/reference:$(OUTPUT_DIR)/CSScriptLibrary.dll /reference:$(OUTPUT_DIR)/NUniversalCharDet.dll /reference:$(OUTPUT_DIR)/SharpCompress.Unsigned.dll /reference:$(OUTPUT_DIR)/PIEHid32Net.dll \
	/reference:System.Core.dll /reference:System.dll \
	/win32icon:$(ICON) $(addprefix /resource:, $(OPEN_BVE_RESOURCE))
	@echo $(COLOR_GREEN)Adding LBA Flag to executable $(COLOR_CYAN)$(OPEN_BVE_OUT)$(COLOR_END)
	@mono $(LBAHEADER_OUT) ${OPEN_BVE_OUT} > /dev/null


##############
# OpenBveApi #
##############

OPEN_BVE_API_FOLDERS  := . Properties
OPEN_BVE_API_FOLDERS  := $(addprefix $(OPEN_BVE_API_ROOT)/, $(OPEN_BVE_API_FOLDERS))
OPEN_BVE_API_SRC      := $(foreach sdir, $(OPEN_BVE_API_FOLDERS), $(wildcard $(sdir)/*.cs))
OPEN_BVE_API_DOC      := $(addprefix /doc:, $(foreach sdir, $(OPEN_BVE_API_FOLDERS), $(wildcard $(sdir)/*.xml)))
OPEN_BVE_API_RESX     := $(foreach sdir, $(OPEN_BVE_API_FOLDERS), $(wildcard $(sdir)/*.resx))
OPEN_BVE_API_RESOURCE := $(addprefix $(OPEN_BVE_API_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(OPEN_BVE_API_ROOT))%.resx, %.resources, $(OPEN_BVE_API_RESX)))))
OPEN_BVE_API_OUT       =$(OUTPUT_DIR)/$(OPEN_BVE_API_FILE)

$(call create_resource, $(OPEN_BVE_API_RESOURCE), $(OPEN_BVE_API_RESX))

$(DEBUG_DIR)/$(OPEN_BVE_API_FILE): $(DEBUG_DEPEND)
$(RELEASE_DIR)/$(OPEN_BVE_API_FILE): $(RELEASE_DEPEND)

$(DEBUG_DIR)/$(OPEN_BVE_API_FILE) $(RELEASE_DIR)/$(OPEN_BVE_API_FILE): $(OPEN_BVE_API_SRC) $(OPEN_BVE_API_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OPEN_BVE_API_OUT)$(COLOR_END)
	@$(CSC) /out:$(OPEN_BVE_API_OUT) /target:library $(OPEN_BVE_API_SRC) $(ARGS) $(OPEN_BVE_API_DOC) \
	/reference:$(OUTPUT_DIR)/CSScriptLibrary.dll /reference:$(OUTPUT_DIR)/NUniversalCharDet.dll /reference:$(OUTPUT_DIR)/SharpCompress.Unsigned.dll \
	/reference:System.Core.dll /reference:System.dll \
	$(addprefix /resource:, $(OPEN_BVE_API_RESOURCE))


##############
# OpenBveAts #
##############

OPEN_BVE_ATS_FOLDERS  := . Properties
OPEN_BVE_ATS_FOLDERS  := $(addprefix $(OPEN_BVE_ATS_ROOT)/, $(OPEN_BVE_ATS_FOLDERS))
OPEN_BVE_ATS_SRC      := $(foreach sdir, $(OPEN_BVE_ATS_FOLDERS), $(wildcard $(sdir)/*.cs))
OPEN_BVE_ATS_DOC      := $(addprefix /doc:, $(foreach sdir, $(OPEN_BVE_ATS_FOLDERS), $(wildcard $(sdir)/*.xml)))
OPEN_BVE_ATS_RESX     := $(foreach sdir, $(OPEN_BVE_ATS_FOLDERS), $(wildcard $(sdir)/*.resx))
OPEN_BVE_ATS_RESOURCE := $(addprefix $(OPEN_BVE_ATS_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(OPEN_BVE_ATS_ROOT))%.resx, %.resources, $(OPEN_BVE_ATS_RESX)))))
OPEN_BVE_ATS_OUT       =$(OUTPUT_DIR)/$(OPEN_BVE_ATS_FILE)

$(call create_resource, $(OPEN_BVE_ATS_RESOURCE), $(OPEN_BVE_ATS_RESX))

$(DEBUG_DIR)/$(OPEN_BVE_ATS_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(OPEN_BVE_ATS_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(OPEN_BVE_ATS_FILE) $(RELEASE_DIR)/$(OPEN_BVE_ATS_FILE): $(OPEN_BVE_ATS_SRC) $(OPEN_BVE_ATS_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OPEN_BVE_ATS_OUT)$(COLOR_END)
	@$(CSC) /out:$(OPEN_BVE_ATS_OUT) /target:library $(OPEN_BVE_ATS_SRC) $(ARGS) $(OPEN_BVE_ATS_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(OPEN_BVE_ATS_RESOURCE))

##############
# Sound.Flac #
##############

SOUND_FLAC_FOLDERS  := . Properties
SOUND_FLAC_FOLDERS  := $(addprefix $(SOUND_FLAC_ROOT)/, $(SOUND_FLAC_FOLDERS))
SOUND_FLAC_SRC      := $(foreach sdir, $(SOUND_FLAC_FOLDERS), $(wildcard $(sdir)/*.cs))
SOUND_FLAC_DOC      := $(addprefix /doc:, $(foreach sdir, $(SOUND_FLAC_FOLDERS), $(wildcard $(sdir)/*.xml)))
SOUND_FLAC_RESX     := $(foreach sdir, $(SOUND_FLAC_FOLDERS), $(wildcard $(sdir)/*.resx))
SOUND_FLAC_RESOURCE := $(addprefix $(SOUND_FLAC_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(SOUND_FLAC_ROOT))%.resx, %.resources, $(SOUND_FLAC_RESX)))))
SOUND_FLAC_OUT       =$(OUTPUT_DIR)/$(SOUND_FLAC_FILE)

$(call create_resource, $(SOUND_FLAC_RESOURCE), $(SOUND_FLAC_RESX))

$(DEBUG_DIR)/$(SOUND_FLAC_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(SOUND_FLAC_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(SOUND_FLAC_FILE) $(RELEASE_DIR)/$(SOUND_FLAC_FILE): $(SOUND_FLAC_SRC) $(SOUND_FLAC_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(SOUND_FLAC_OUT)$(COLOR_END)
	@$(CSC) /out:$(SOUND_FLAC_OUT) /target:library $(SOUND_FLAC_SRC) $(ARGS) $(SOUND_FLAC_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(SOUND_FLAC_RESOURCE))

##################
# Sound.RiffWave #
##################

SOUND_RIFFWAVE_FOLDERS  := . Properties
SOUND_RIFFWAVE_FOLDERS  := $(addprefix $(SOUND_RIFFWAVE_ROOT)/, $(SOUND_RIFFWAVE_FOLDERS))
SOUND_RIFFWAVE_SRC      := $(foreach sdir, $(SOUND_RIFFWAVE_FOLDERS), $(wildcard $(sdir)/*.cs))
SOUND_RIFFWAVE_DOC      := $(addprefix /doc:, $(foreach sdir, $(SOUND_RIFFWAVE_FOLDERS), $(wildcard $(sdir)/*.xml)))
SOUND_RIFFWAVE_RESX     := $(foreach sdir, $(SOUND_RIFFWAVE_FOLDERS), $(wildcard $(sdir)/*.resx))
SOUND_RIFFWAVE_RESOURCE := $(addprefix $(SOUND_RIFFWAVE_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(SOUND_RIFFWAVE_ROOT))%.resx, %.resources, $(SOUND_RIFFWAVE_RESX)))))
SOUND_RIFFWAVE_OUT       =$(OUTPUT_DIR)/$(SOUND_RIFFWAVE_FILE)

$(call create_resource, $(SOUND_RIFFWAVE_RESOURCE), $(SOUND_RIFFWAVE_RESX))

$(DEBUG_DIR)/$(SOUND_RIFFWAVE_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(SOUND_RIFFWAVE_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(SOUND_RIFFWAVE_FILE) $(RELEASE_DIR)/$(SOUND_RIFFWAVE_FILE): $(SOUND_RIFFWAVE_SRC) $(SOUND_RIFFWAVE_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(SOUND_RIFFWAVE_OUT)$(COLOR_END)
	@$(CSC) /out:$(SOUND_RIFFWAVE_OUT) /target:library $(SOUND_RIFFWAVE_SRC) $(ARGS) $(SOUND_RIFFWAVE_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(SOUND_RIFFWAVE_RESOURCE))

###############
# Texture.Ace #
###############

TEXTURE_ACE_FOLDERS  := . Properties
TEXTURE_ACE_FOLDERS  := $(addprefix $(TEXTURE_ACE_ROOT)/, $(TEXTURE_ACE_FOLDERS))
TEXTURE_ACE_SRC      := $(foreach sdir, $(TEXTURE_ACE_FOLDERS), $(wildcard $(sdir)/*.cs))
TEXTURE_ACE_DOC      := $(addprefix /doc:, $(foreach sdir, $(TEXTURE_ACE_FOLDERS), $(wildcard $(sdir)/*.xml)))
TEXTURE_ACE_RESX     := $(foreach sdir, $(TEXTURE_ACE_FOLDERS), $(wildcard $(sdir)/*.resx))
TEXTURE_ACE_RESOURCE := $(addprefix $(TEXTURE_ACE_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(TEXTURE_ACE_ROOT))%.resx, %.resources, $(TEXTURE_ACE_RESX)))))
TEXTURE_ACE_OUT       =$(OUTPUT_DIR)/$(TEXTURE_ACE_FILE)

$(call create_resource, $(TEXTURE_ACE_RESOURCE), $(TEXTURE_ACE_RESX))

$(DEBUG_DIR)/$(TEXTURE_ACE_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(TEXTURE_ACE_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(TEXTURE_ACE_FILE) $(RELEASE_DIR)/$(TEXTURE_ACE_FILE): $(TEXTURE_ACE_SRC) $(TEXTURE_ACE_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TEXTURE_ACE_OUT)$(COLOR_END)
	@$(CSC) /out:$(TEXTURE_ACE_OUT) /target:library $(TEXTURE_ACE_SRC) $(ARGS) $(TEXTURE_ACE_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(TEXTURE_ACE_RESOURCE))

#############################
# Texture.BmpGifJpegPngTiff #
#############################

TEXTURE_BGJPT_FOLDERS  := . Properties
TEXTURE_BGJPT_FOLDERS  := $(addprefix $(TEXTURE_BGJPT_ROOT)/, $(TEXTURE_BGJPT_FOLDERS))
TEXTURE_BGJPT_SRC      := $(foreach sdir, $(TEXTURE_BGJPT_FOLDERS), $(wildcard $(sdir)/*.cs))
TEXTURE_BGJPT_DOC      := $(addprefix /doc:, $(foreach sdir, $(TEXTURE_BGJPT_FOLDERS), $(wildcard $(sdir)/*.xml)))
TEXTURE_BGJPT_RESX     := $(foreach sdir, $(TEXTURE_BGJPT_FOLDERS), $(wildcard $(sdir)/*.resx))
TEXTURE_BGJPT_RESOURCE := $(addprefix $(TEXTURE_BGJPT_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(TEXTURE_BGJPT_ROOT))%.resx, %.resources, $(TEXTURE_BGJPT_RESX)))))
TEXTURE_BGJPT_OUT       =$(OUTPUT_DIR)/$(TEXTURE_BGJPT_FILE)

$(call create_resource, $(TEXTURE_BGJPT_RESOURCE), $(TEXTURE_BGJPT_RESX))

$(DEBUG_DIR)/$(TEXTURE_BGJPT_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(TEXTURE_BGJPT_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(TEXTURE_BGJPT_FILE) $(RELEASE_DIR)/$(TEXTURE_BGJPT_FILE): $(TEXTURE_BGJPT_SRC) $(TEXTURE_BGJPT_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TEXTURE_BGJPT_OUT)$(COLOR_END)
	@$(CSC) /out:$(TEXTURE_BGJPT_OUT) /target:library $(TEXTURE_BGJPT_SRC) $(ARGS) $(TEXTURE_BGJPT_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(TEXTURE_BGJPT_RESOURCE))

###############
# Texture.Dds #
###############

TEXTURE_DDS_FOLDERS  := . Properties
TEXTURE_DDS_FOLDERS  := $(addprefix $(TEXTURE_DDS_ROOT)/, $(TEXTURE_DDS_FOLDERS))
TEXTURE_DDS_SRC      := $(foreach sdir, $(TEXTURE_DDS_FOLDERS), $(wildcard $(sdir)/*.cs))
TEXTURE_DDS_DOC      := $(addprefix /doc:, $(foreach sdir, $(TEXTURE_DDS_FOLDERS), $(wildcard $(sdir)/*.xml)))
TEXTURE_DDS_RESX     := $(foreach sdir, $(TEXTURE_DDS_FOLDERS), $(wildcard $(sdir)/*.resx))
TEXTURE_DDS_RESOURCE := $(addprefix $(TEXTURE_DDS_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(TEXTURE_DDS_ROOT))%.resx, %.resources, $(TEXTURE_DDS_RESX)))))
TEXTURE_DDS_OUT       =$(OUTPUT_DIR)/$(TEXTURE_DDS_FILE)

$(call create_resource, $(TEXTURE_DDS_RESOURCE), $(TEXTURE_DDS_RESX))

$(DEBUG_DIR)/$(TEXTURE_DDS_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(TEXTURE_DDS_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(TEXTURE_DDS_FILE) $(RELEASE_DIR)/$(TEXTURE_DDS_FILE): $(TEXTURE_DDS_SRC) $(TEXTURE_DDS_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TEXTURE_DDS_OUT)$(COLOR_END)
	@$(CSC) /out:$(TEXTURE_DDS_OUT) /target:library $(TEXTURE_DDS_SRC) $(ARGS) $(TEXTURE_DDS_DOC) \
	/reference:$(OPEN_BVE_API_OUT) /reference:System.Core.dll $(addprefix /resource:, $(TEXTURE_DDS_RESOURCE))

###############
# RouteViewer #
###############

ROUTE_VIEWER_FOLDERS  := . Audio Parsers Properties System System/Functions
ROUTE_VIEWER_FOLDERS  := $(addprefix $(ROUTE_VIEWER_ROOT)/, $(ROUTE_VIEWER_FOLDERS))
ROUTE_VIEWER_SRC      := $(foreach sdir, $(ROUTE_VIEWER_FOLDERS), $(wildcard $(sdir)/*.cs))
ROUTE_VIEWER_DOC      := $(addprefix /doc:, $(foreach sdir, $(ROUTE_VIEWER_FOLDERS), $(wildcard $(sdir)/*.xml)))
ROUTE_VIEWER_RESX     := $(foreach sdir, $(ROUTE_VIEWER_FOLDERS), $(wildcard $(sdir)/*.resx))
ROUTE_VIEWER_RESOURCE := $(addprefix $(ROUTE_VIEWER_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(ROUTE_VIEWER_ROOT))%.resx, %.resources, $(ROUTE_VIEWER_RESX)))))
ROUTE_VIEWER_OUT       =$(OUTPUT_DIR)/$(ROUTE_VIEWER_FILE)

$(call create_resource, $(ROUTE_VIEWER_RESOURCE), $(ROUTE_VIEWER_RESX))

$(DEBUG_DIR)/$(ROUTE_VIEWER_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(ROUTE_VIEWER_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(ROUTE_VIEWER_FILE) $(RELEASE_DIR)/$(ROUTE_VIEWER_FILE): $(ROUTE_VIEWER_SRC) $(ROUTE_VIEWER_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(ROUTE_VIEWER_OUT)$(COLOR_END)
	@$(CSC) /out:$(ROUTE_VIEWER_OUT) /target:winexe /main:OpenBve.Program $(ROUTE_VIEWER_SRC) $(ARGS) $(ROUTE_VIEWER_DOC) \
	/reference:$(OPEN_BVE_API_OUT) /reference:$(OUTPUT_DIR)/OpenTK \
	/reference:System.Core.dll /reference:System.dll \
	/win32icon:$(ICON) $(addprefix /resource:, $(ROUTE_VIEWER_RESOURCE))

################
# ObjectBender #
################

OBJECT_BENDER_FOLDERS  := . Properties
OBJECT_BENDER_FOLDERS  := $(addprefix $(OBJECT_BENDER_ROOT)/, $(OBJECT_BENDER_FOLDERS))
OBJECT_BENDER_SRC      := $(foreach sdir, $(OBJECT_BENDER_FOLDERS), $(wildcard $(sdir)/*.cs))
OBJECT_BENDER_DOC      := $(addprefix /doc:, $(foreach sdir, $(OBJECT_BENDER_FOLDERS), $(wildcard $(sdir)/*.xml)))
OBJECT_BENDER_RESX     := $(foreach sdir, $(OBJECT_BENDER_FOLDERS), $(wildcard $(sdir)/*.resx))
OBJECT_BENDER_RESOURCE := $(addprefix $(OBJECT_BENDER_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(OBJECT_BENDER_ROOT))%.resx, %.resources, $(OBJECT_BENDER_RESX)))))
OBJECT_BENDER_OUT       =$(OUTPUT_DIR)/$(OBJECT_BENDER_FILE)

$(call create_resource, $(OBJECT_BENDER_RESOURCE), $(OBJECT_BENDER_RESX))

$(DEBUG_DIR)/$(OBJECT_BENDER_FILE) $(RELEASE_DIR)/$(OBJECT_BENDER_FILE): $(OBJECT_BENDER_SRC) $(OBJECT_BENDER_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OBJECT_BENDER_OUT)$(COLOR_END)
	@$(CSC) /out:$(OBJECT_BENDER_OUT) /target:winexe /main:ObjectBender.Program $(OBJECT_BENDER_SRC) $(ARGS) $(OBJECT_BENDER_DOC) \
	/win32icon:$(ICON) $(addprefix /resource:, $(OBJECT_BENDER_RESOURCE))
	
###################
# CarXMLConvertor #
###################

CAR_XML_FOLDERS  := . Properties
CAR_XML_FOLDERS  := $(addprefix $(CAR_XML_ROOT)/, $(CAR_XML_FOLDERS))
CAR_XML_SRC      := $(foreach sdir, $(CAR_XML_FOLDERS), $(wildcard $(sdir)/*.cs))
CAR_XML_DOC      := $(addprefix /doc:, $(foreach sdir, $(CAR_XML_FOLDERS), $(wildcard $(sdir)/*.xml)))
CAR_XML_RESX     := $(foreach sdir, $(CAR_XML_FOLDERS), $(wildcard $(sdir)/*.resx))
CAR_XML_RESOURCE := $(addprefix $(CAR_XML_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(CAR_XML_ROOT))%.resx, %.resources, $(CAR_XML_RESX)))))
CAR_XML_OUT       =$(OUTPUT_DIR)/$(CAR_XML_FILE)

$(call create_resource, $(CAR_XML_RESOURCE), $(CAR_XML_RESX))

$(DEBUG_DIR)/$(CAR_XML_FILE) $(RELEASE_DIR)/$(CAR_XML_FILE): $(CAR_XML_SRC) $(CAR_XML_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(CAR_XML_OUT)$(COLOR_END)
	@$(CSC) /out:$(CAR_XML_OUT) /target:winexe /main:CarXmlConvertor.Program $(CAR_XML_SRC) $(ARGS) $(CAR_XML_DOC) \
	/reference:$(OPEN_BVE_API_OUT) /win32icon:$(ICON) $(addprefix /resource:, $(CAR_XML_RESOURCE))

################
# ObjectViewer #
################

OBJECT_VIEWER_FOLDERS  := . Parsers Properties System
OBJECT_VIEWER_FOLDERS  := $(addprefix $(OBJECT_VIEWER_ROOT)/, $(OBJECT_VIEWER_FOLDERS))
OBJECT_VIEWER_SRC      := $(foreach sdir, $(OBJECT_VIEWER_FOLDERS), $(wildcard $(sdir)/*.cs))
OBJECT_VIEWER_DOC      := $(addprefix /doc:, $(foreach sdir, $(OBJECT_VIEWER_FOLDERS), $(wildcard $(sdir)/*.xml)))
OBJECT_VIEWER_RESX     := $(foreach sdir, $(OBJECT_VIEWER_FOLDERS), $(wildcard $(sdir)/*.resx))
OBJECT_VIEWER_RESOURCE := $(addprefix $(OBJECT_VIEWER_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(OBJECT_VIEWER_ROOT))%.resx, %.resources, $(OBJECT_VIEWER_RESX)))))
OBJECT_VIEWER_OUT       =$(OUTPUT_DIR)/$(OBJECT_VIEWER_FILE)

$(call create_resource, $(OBJECT_VIEWER_RESOURCE), $(OBJECT_VIEWER_RESX))

$(DEBUG_DIR)/$(OBJECT_VIEWER_FILE): $(DEBUG_DIR)/$(OPEN_BVE_API_FILE)
$(RELEASE_DIR)/$(OBJECT_VIEWER_FILE): $(RELEASE_DIR)/$(OPEN_BVE_API_FILE)

$(DEBUG_DIR)/$(OBJECT_VIEWER_FILE) $(RELEASE_DIR)/$(OBJECT_VIEWER_FILE): $(OBJECT_VIEWER_SRC) $(OBJECT_VIEWER_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OBJECT_VIEWER_OUT)$(COLOR_END)
	@$(CSC) /out:$(OBJECT_VIEWER_OUT) /target:winexe /main:OpenBve.Program $(OBJECT_VIEWER_SRC) $(ARGS) $(OBJECT_VIEWER_DOC) \
	/reference:$(OPEN_BVE_API_OUT) /reference:$(OUTPUT_DIR)/OpenTK.dll /reference:System.Core.dll \
	/win32icon:$(ICON) $(addprefix /resource:, $(OBJECT_VIEWER_RESOURCE))

###############
# TrainEditor #
###############

TRAIN_EDITOR_FOLDERS  := . CsvB3dDecoder Properties TrainsimApi/Codecs TrainsimApi/Geometry TrainsimApi/Platform TrainsimApi/Vectors
TRAIN_EDITOR_FOLDERS  := $(addprefix $(TRAIN_EDITOR_ROOT)/, $(TRAIN_EDITOR_FOLDERS))
TRAIN_EDITOR_SRC      := $(foreach sdir, $(TRAIN_EDITOR_FOLDERS), $(wildcard $(sdir)/*.cs))
TRAIN_EDITOR_DOC      := $(addprefix /doc:, $(foreach sdir, $(TRAIN_EDITOR_FOLDERS), $(wildcard $(sdir)/*.xml)))
TRAIN_EDITOR_RESX     := $(foreach sdir, $(TRAIN_EDITOR_FOLDERS), $(wildcard $(sdir)/*.resx))
TRAIN_EDITOR_RESOURCE := $(addprefix $(TRAIN_EDITOR_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(TRAIN_EDITOR_ROOT))%.resx, %.resources, $(TRAIN_EDITOR_RESX)))))
TRAIN_EDITOR_OUT       =$(OUTPUT_DIR)/$(TRAIN_EDITOR_FILE)

$(call create_resource, $(TRAIN_EDITOR_RESOURCE), $(TRAIN_EDITOR_RESX))

$(DEBUG_DIR)/$(TRAIN_EDITOR_FILE) $(RELEASE_DIR)/$(TRAIN_EDITOR_FILE): $(TRAIN_EDITOR_SRC) $(TRAIN_EDITOR_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TRAIN_EDITOR_OUT)$(COLOR_END)
	@$(CSC) /out:$(TRAIN_EDITOR_OUT) /target:winexe /main:TrainEditor.Program $(TRAIN_EDITOR_SRC) $(ARGS) $(TRAIN_EDITOR_DOC) \
	/win32icon:$(ICON) $(addprefix /resource:, $(TRAIN_EDITOR_RESOURCE))

#############
# LBAHeader #
#############

LBAHEADER_FOLDERS  := .
LBAHEADER_FOLDERS  := $(addprefix $(LBAHEADER_ROOT)/, $(LBAHEADER_FOLDERS))
LBAHEADER_SRC      := $(foreach sdir, $(LBAHEADER_FOLDERS), $(wildcard $(sdir)/*.cs))
LBAHEADER_OUT       =$(OUTPUT_DIR)/$(LBAHEADER_FILE)

$(DEBUG_DIR)/$(LBAHEADER_FILE) $(RELEASE_DIR)/$(LBAHEADER_FILE): $(LBAHEADER_SRC)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(LBAHEADER_OUT)$(COLOR_END)
	@$(CSC) /out:$(LBAHEADER_OUT) /target:winexe /main:LBAHeader.FixLBAHeader $(LBAHEADER_SRC) $(ARGS)