# C-Sharp Compiler
CSC := gmcs -pkg:dotnet -lib:/usr/lib/mono/4.0

# Resource file creator
RESGEN := resgen

# Standard Arugments
DEBUG_ARGS   := /noconfig /debug:Full /debug+ /optimize- /warnaserror- /unsafe+ /define:"DEBUG;TRACE" /platform:x86 /warn:4 /pkg:dotnet
RELEASE_ARGS := /noconfig /debug- /optimize+ /unsafe+ /checked- /define:"TRACE" /platform:x86 /warn:4 /pkg:dotnet

# Directories
DEBUG_DIR   := bin_debug
RELEASE_DIR := bin_release
OUTPUT_DIR  := $(DEBUG_DIR)

# Current Args
ARGS := $(DEBUG_ARGS)

# Thumbnail
ICON := source/icon.ico

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
.PHONY: debug
.PHONY: release 
.PHONY: clean
.PHONY: clean-all

openbve: prep_dirs
openbve: $(OUTPUT_DIR)/OpenBve.exe
openbve: $(OUTPUT_DIR)/OpenBveApi.dll
openbve: $(OUTPUT_DIR)/Data/Plugins/OpenBveAts.dll
openbve: $(OUTPUT_DIR)/Data/Plugins/Sound.Flac.dll
openbve: $(OUTPUT_DIR)/Data/Plugins/Sound.RiffWave.dll
openbve: $(OUTPUT_DIR)/Data/Plugins/Texture.Ace.dll
openbve: $(OUTPUT_DIR)/Data/Plugins/Texture.BmpGifJpegPngTiff.dll
openbve: copy_depends

debug: openbve-debug

release: openbve-release

openbve-debug: openbve

openbve-release: ARGS := $(RELEASE_ARGS)
openbve-release: OUTPUT_DIR := $(RELEASE_DIR)
openbve-release: openbve

all-debug: all

all: prep_dirs
all: $(OUTPUT_DIR)/OpenBve.exe
all: $(OUTPUT_DIR)/ObjectBender.exe
all: $(OUTPUT_DIR)/ObjectViewer.exe
all: $(OUTPUT_DIR)/RouteViewer.exe
all: $(OUTPUT_DIR)/TrainEditor.exe
all: $(OUTPUT_DIR)/OpenBveApi.dll
all: $(OUTPUT_DIR)/Data/Plugins/OpenBveAts.dll
all: $(OUTPUT_DIR)/Data/Plugins/Sound.Flac.dll
all: $(OUTPUT_DIR)/Data/Plugins/Sound.RiffWave.dll
all: $(OUTPUT_DIR)/Data/Plugins/Texture.Ace.dll
all: $(OUTPUT_DIR)/Data/Plugins/Texture.BmpGifJpegPngTiff.dll
all: copy_depends

all-release: prep_dirs
all-release: ARGS := $(RELEASE_ARGS)
all-release: OUTPUT_DIR := $(RELEASE_DIR)
all-release: all

CP_UPDATE_FLAG = -u
CP_RECURSE = -r
ifeq ($(shell uname -s),Darwin) 
    CP_UPDATE_FLAG = 
    CP_RECURSE = -R
endif 

prep_dirs: 
	@echo $(COLOR_BLUE)Prepping $(OUTPUT_DIR)...$(COLOR_END)
	@mkdir -p $(OUTPUT_DIR)
	@echo $(COLOR_BLUE)Making plugin folder$(COLOR_END)
	@mkdir -p $(OUTPUT_DIR)/Data/Plugins/
	@echo $(COLOR_BLUE)Copying dependencies$(COLOR_END)
	@cp $(CP_UPDATE_FLAG) dependencies/* $(OUTPUT_DIR)

copy_depends:
	@echo $(COLOR_BLUE)Copying data$(COLOR_END)
	@cp -r $(CP_UPDATE_FLAG) $(OPEN_BVE_ROOT)/Data $(OUTPUT_DIR)

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

	# Resource Files
	rm -f `find . | grep .resources | tr '\n' ' '`

	# Assembly
	rm -f $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs

clean-all:
	rm -rf bin*/
	rm -f `find . | grep .resources | tr '\n' ' '`
	
	# Assembly
	rm -f $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs

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

OPEN_BVE_ROOT     := source/OpenBVE
OPEN_BVE_FOLDERS  := . Audio Game Graphics Graphics/Renderer Interface OldCode OldCode/NewCode Parsers Properties OldParsers OldParsers/BveRouteParser Simulation/TrainManager Simulation/TrainManager/Train Simulation/World System System/Functions System/Input System/Logging System/Program System/Translations UserInterface
OPEN_BVE_FOLDERS  := $(addprefix $(OPEN_BVE_ROOT)/, $(OPEN_BVE_FOLDERS))
OPEN_BVE_SRC      := $(patsubst %, "%", $(foreach sdir, $(OPEN_BVE_FOLDERS), $(wildcard $(sdir)/*.cs)))
OPEN_BVE_DOC      := $(addprefix /doc:, $(foreach sdir, $(OPEN_BVE_FOLDERS), $(wildcard $(sdir)/*.xml)))
OPEN_BVE_RESX     := $(foreach sdir, $(OPEN_BVE_FOLDERS), $(wildcard $(sdir)/*.resx))
OPEN_BVE_RESOURCE := $(addprefix $(OPEN_BVE_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst $(dir $(OPEN_BVE_ROOT))%.resx, %.resources, $(OPEN_BVE_RESX)))))
OPEN_BVE_OUT       = $(OUTPUT_DIR)/OpenBve.exe

$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs: $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.sh
	@echo $(COLOR_WHITE)Creating assembly file $(COLOR_CYAN)$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs$(COLOR_END)
	@bash $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.sh

$(call create_resource, $(OPEN_BVE_RESOURCE), $(OPEN_BVE_RESX))

$(OPEN_BVE_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs $(patsubst "%", %, $(OPEN_BVE_SRC)) $(OPEN_BVE_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OPEN_BVE_OUT)$(COLOR_END)
	@$(CSC) /out:$(OPEN_BVE_OUT) /target:winexe /main:OpenBve.Program $(OPEN_BVE_SRC) $(ARGS) $(OPEN_BVE_DOC) \
	$(OPEN_BVE_ROOT)/Properties/AssemblyInfo.cs \
	/reference:$(OUTPUT_DIR)/OpenTK.dll /reference:$(OPEN_BVE_API_OUT) \
	/reference:$(OUTPUT_DIR)/CSScriptLibrary.dll /reference:$(OUTPUT_DIR)/NUniversalCharDet.dll /reference:$(OUTPUT_DIR)/SharpCompress.Unsigned.dll \
	/reference:System.Core.dll /reference:System.dll \
	/win32icon:$(ICON) $(addprefix /resource:, $(OPEN_BVE_RESOURCE))


##############
# OpenBveApi #
##############

OPEN_BVE_API_ROOT     := source/OpenBveApi
OPEN_BVE_API_FOLDERS  := . Properties
OPEN_BVE_API_FOLDERS  := $(addprefix $(OPEN_BVE_API_ROOT)/, $(OPEN_BVE_API_FOLDERS))
OPEN_BVE_API_SRC      := $(foreach sdir, $(OPEN_BVE_API_FOLDERS), $(wildcard $(sdir)/*.cs))
OPEN_BVE_API_DOC      := $(addprefix /doc:, $(foreach sdir, $(OPEN_BVE_API_FOLDERS), $(wildcard $(sdir)/*.xml)))
OPEN_BVE_API_RESX     := $(foreach sdir, $(OPEN_BVE_API_FOLDERS), $(wildcard $(sdir)/*.resx))
OPEN_BVE_API_RESOURCE := $(addprefix $(OPEN_BVE_API_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(OPEN_BVE_API_RESX)))))
OPEN_BVE_API_OUT       = $(OUTPUT_DIR)/OpenBveApi.dll

$(call create_resource, $(OPEN_BVE_API_RESOURCE), $(OPEN_BVE_API_RESX))

$(OPEN_BVE_API_OUT): $(OPEN_BVE_API_SRC) $(OPEN_BVE_API_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OPEN_BVE_API_OUT)$(COLOR_END)
	@$(CSC) /out:$(OPEN_BVE_API_OUT) /target:library $(OPEN_BVE_API_SRC) $(ARGS) $(OPEN_BVE_API_DOC) \
	/reference:$(OUTPUT_DIR)/CSScriptLibrary.dll /reference:$(OUTPUT_DIR)/NUniversalCharDet.dll /reference:$(OUTPUT_DIR)/SharpCompress.Unsigned.dll \
	/reference:System.Core.dll /reference:System.dll \
	$(addprefix /resource:, $(OPEN_BVE_API_RESOURCE))


##############
# OpenBveAts #
##############

OPEN_BVE_ATS_ROOT     := source/OpenBveAts
OPEN_BVE_ATS_FOLDERS  := . Properties
OPEN_BVE_ATS_FOLDERS  := $(addprefix $(OPEN_BVE_ATS_ROOT)/, $(OPEN_BVE_ATS_FOLDERS))
OPEN_BVE_ATS_SRC      := $(foreach sdir, $(OPEN_BVE_ATS_FOLDERS), $(wildcard $(sdir)/*.cs))
OPEN_BVE_ATS_DOC      := $(addprefix /doc:, $(foreach sdir, $(OPEN_BVE_ATS_FOLDERS), $(wildcard $(sdir)/*.xml)))
OPEN_BVE_ATS_RESX     := $(foreach sdir, $(OPEN_BVE_ATS_FOLDERS), $(wildcard $(sdir)/*.resx))
OPEN_BVE_ATS_RESOURCE := $(addprefix $(OPEN_BVE_ATS_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(OPEN_BVE_ATS_RESX)))))
OPEN_BVE_ATS_OUT       = $(OUTPUT_DIR)/Data/Plugins/OpenBveAts.dll

$(call create_resource, $(OPEN_BVE_ATS_RESOURCE), $(OPEN_BVE_ATS_RESX))

$(OPEN_BVE_ATS_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(OPEN_BVE_ATS_SRC) $(OPEN_BVE_ATS_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OPEN_BVE_ATS_OUT)$(COLOR_END)
	@$(CSC) /out:$(OPEN_BVE_ATS_OUT) /target:library $(OPEN_BVE_ATS_SRC) $(ARGS) $(OPEN_BVE_ATS_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(OPEN_BVE_ATS_RESOURCE))

##############
# Sound.Flac #
##############

SOUND_FLAC_ROOT     := source/Sound.Flac
SOUND_FLAC_FOLDERS  := . Properties
SOUND_FLAC_FOLDERS  := $(addprefix $(SOUND_FLAC_ROOT)/, $(SOUND_FLAC_FOLDERS))
SOUND_FLAC_SRC      := $(foreach sdir, $(SOUND_FLAC_FOLDERS), $(wildcard $(sdir)/*.cs))
SOUND_FLAC_DOC      := $(addprefix /doc:, $(foreach sdir, $(SOUND_FLAC_FOLDERS), $(wildcard $(sdir)/*.xml)))
SOUND_FLAC_RESX     := $(foreach sdir, $(SOUND_FLAC_FOLDERS), $(wildcard $(sdir)/*.resx))
SOUND_FLAC_RESOURCE := $(addprefix $(SOUND_FLAC_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(SOUND_FLAC_RESX)))))
SOUND_FLAC_OUT       = $(OUTPUT_DIR)/Data/Plugins/Sound.Flac.dll

$(call create_resource, $(SOUND_FLAC_RESOURCE), $(SOUND_FLAC_RESX))

$(SOUND_FLAC_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(SOUND_FLAC_SRC) $(SOUND_FLAC_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(SOUND_FLAC_OUT)$(COLOR_END)
	@$(CSC) /out:$(SOUND_FLAC_OUT) /target:library $(SOUND_FLAC_SRC) $(ARGS) $(SOUND_FLAC_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(SOUND_FLAC_RESOURCE))

##################
# Sound.RiffWave #
##################

SOUND_RIFFWAVE_ROOT     := source/Sound.RiffWave
SOUND_RIFFWAVE_FOLDERS  := . Properties
SOUND_RIFFWAVE_FOLDERS  := $(addprefix $(SOUND_RIFFWAVE_ROOT)/, $(SOUND_RIFFWAVE_FOLDERS))
SOUND_RIFFWAVE_SRC      := $(foreach sdir, $(SOUND_RIFFWAVE_FOLDERS), $(wildcard $(sdir)/*.cs))
SOUND_RIFFWAVE_DOC      := $(addprefix /doc:, $(foreach sdir, $(SOUND_RIFFWAVE_FOLDERS), $(wildcard $(sdir)/*.xml)))
SOUND_RIFFWAVE_RESX     := $(foreach sdir, $(SOUND_RIFFWAVE_FOLDERS), $(wildcard $(sdir)/*.resx))
SOUND_RIFFWAVE_RESOURCE := $(addprefix $(SOUND_RIFFWAVE_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(SOUND_RIFFWAVE_RESX)))))
SOUND_RIFFWAVE_OUT       = $(OUTPUT_DIR)/Data/Plugins/Sound.RiffWave.dll

$(call create_resource, $(SOUND_RIFFWAVE_RESOURCE), $(SOUND_RIFFWAVE_RESX))

$(SOUND_RIFFWAVE_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(SOUND_RIFFWAVE_SRC) $(SOUND_RIFFWAVE_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(SOUND_RIFFWAVE_OUT)$(COLOR_END)
	@$(CSC) /out:$(SOUND_RIFFWAVE_OUT) /target:library $(SOUND_RIFFWAVE_SRC) $(ARGS) $(SOUND_RIFFWAVE_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(SOUND_RIFFWAVE_RESOURCE))

###############
# Texture.Ace #
###############

TEXTURE_ACE_ROOT     := source/Texture.Ace
TEXTURE_ACE_FOLDERS  := . Properties
TEXTURE_ACE_FOLDERS  := $(addprefix $(TEXTURE_ACE_ROOT)/, $(TEXTURE_ACE_FOLDERS))
TEXTURE_ACE_SRC      := $(foreach sdir, $(TEXTURE_ACE_FOLDERS), $(wildcard $(sdir)/*.cs))
TEXTURE_ACE_DOC      := $(addprefix /doc:, $(foreach sdir, $(TEXTURE_ACE_FOLDERS), $(wildcard $(sdir)/*.xml)))
TEXTURE_ACE_RESX     := $(foreach sdir, $(TEXTURE_ACE_FOLDERS), $(wildcard $(sdir)/*.resx))
TEXTURE_ACE_RESOURCE := $(addprefix $(TEXTURE_ACE_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(TEXTURE_ACE_RESX)))))
TEXTURE_ACE_OUT       = $(OUTPUT_DIR)/Data/Plugins/Texture.Ace.dll

$(call create_resource, $(TEXTURE_ACE_RESOURCE), $(TEXTURE_ACE_RESX))

$(TEXTURE_ACE_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(TEXTURE_ACE_SRC) $(TEXTURE_ACE_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TEXTURE_ACE_OUT)$(COLOR_END)
	@$(CSC) /out:$(TEXTURE_ACE_OUT) /target:library $(TEXTURE_ACE_SRC) $(ARGS) $(TEXTURE_ACE_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(TEXTURE_ACE_RESOURCE))

#############################
# Texture.BmpGifJpegPngTiff #
#############################

TEXTURE_BGJPT_ROOT     := source/Texture.BmpGifJpegPngTiff
TEXTURE_BGJPT_FOLDERS  := . Properties
TEXTURE_BGJPT_FOLDERS  := $(addprefix $(TEXTURE_BGJPT_ROOT)/, $(TEXTURE_BGJPT_FOLDERS))
TEXTURE_BGJPT_SRC      := $(foreach sdir, $(TEXTURE_BGJPT_FOLDERS), $(wildcard $(sdir)/*.cs))
TEXTURE_BGJPT_DOC      := $(addprefix /doc:, $(foreach sdir, $(TEXTURE_BGJPT_FOLDERS), $(wildcard $(sdir)/*.xml)))
TEXTURE_BGJPT_RESX     := $(foreach sdir, $(TEXTURE_BGJPT_FOLDERS), $(wildcard $(sdir)/*.resx))
TEXTURE_BGJPT_RESOURCE := $(addprefix $(TEXTURE_BGJPT_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(TEXTURE_BGJPT_RESX)))))
TEXTURE_BGJPT_OUT       = $(OUTPUT_DIR)/Data/Plugins/Texture.BmpGifJpegPngTiff.dll

$(call create_resource, $(TEXTURE_BGJPT_RESOURCE), $(TEXTURE_BGJPT_RESX))

$(TEXTURE_BGJPT_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(TEXTURE_BGJPT_SRC) $(TEXTURE_BGJPT_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TEXTURE_BGJPT_OUT)$(COLOR_END)
	@$(CSC) /out:$(TEXTURE_BGJPT_OUT) /target:library $(TEXTURE_BGJPT_SRC) $(ARGS) $(TEXTURE_BGJPT_DOC) \
	/reference:$(OPEN_BVE_API_OUT) $(addprefix /resource:, $(TEXTURE_BGJPT_RESOURCE))


###############
# RouteViewer #
###############

ROUTE_VIEWER_ROOT     := source/RouteViewer
ROUTE_VIEWER_FOLDERS  := . Parsers Properties System
ROUTE_VIEWER_FOLDERS  := $(addprefix $(ROUTE_VIEWER_ROOT)/, $(ROUTE_VIEWER_FOLDERS))
ROUTE_VIEWER_SRC      := $(foreach sdir, $(ROUTE_VIEWER_FOLDERS), $(wildcard $(sdir)/*.cs))
ROUTE_VIEWER_DOC      := $(addprefix /doc:, $(foreach sdir, $(ROUTE_VIEWER_FOLDERS), $(wildcard $(sdir)/*.xml)))
ROUTE_VIEWER_RESX     := $(foreach sdir, $(ROUTE_VIEWER_FOLDERS), $(wildcard $(sdir)/*.resx))
ROUTE_VIEWER_RESOURCE := $(addprefix $(ROUTE_VIEWER_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(ROUTE_VIEWER_RESX)))))
ROUTE_VIEWER_OUT       = $(OUTPUT_DIR)/RouteViewer.exe

$(call create_resource, $(ROUTE_VIEWER_RESOURCE), $(ROUTE_VIEWER_RESX))

$(ROUTE_VIEWER_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(ROUTE_VIEWER_SRC) $(ROUTE_VIEWER_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(ROUTE_VIEWER_OUT)$(COLOR_END)
	@$(CSC) /out:$(ROUTE_VIEWER_OUT) /target:winexe /main:OpenBve.Program $(ROUTE_VIEWER_SRC) $(ARGS) $(ROUTE_VIEWER_DOC) \
	/reference:$(OPEN_BVE_API_OUT) /reference:$(OUTPUT_DIR)/OpenTK \
	/win32icon:$(ICON) $(addprefix /resource:, $(ROUTE_VIEWER_RESOURCE))

################
# ObjectBender #
################

OBJECT_BENDER_ROOT     := source/ObjectBender
OBJECT_BENDER_FOLDERS  := . Properties
OBJECT_BENDER_FOLDERS  := $(addprefix $(OBJECT_BENDER_ROOT)/, $(OBJECT_BENDER_FOLDERS))
OBJECT_BENDER_SRC      := $(foreach sdir, $(OBJECT_BENDER_FOLDERS), $(wildcard $(sdir)/*.cs))
OBJECT_BENDER_DOC      := $(addprefix /doc:, $(foreach sdir, $(OBJECT_BENDER_FOLDERS), $(wildcard $(sdir)/*.xml)))
OBJECT_BENDER_RESX     := $(foreach sdir, $(OBJECT_BENDER_FOLDERS), $(wildcard $(sdir)/*.resx))
OBJECT_BENDER_RESOURCE := $(addprefix $(OBJECT_BENDER_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(OBJECT_BENDER_RESX)))))
OBJECT_BENDER_OUT       = $(OUTPUT_DIR)/ObjectBender.exe

$(call create_resource, $(OBJECT_BENDER_RESOURCE), $(OBJECT_BENDER_RESX))

$(OBJECT_BENDER_OUT): $(OBJECT_BENDER_SRC) $(OBJECT_BENDER_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OBJECT_BENDER_OUT)$(COLOR_END)
	@$(CSC) /out:$(OBJECT_BENDER_OUT) /target:winexe /main:ObjectBender.Program $(OBJECT_BENDER_SRC) $(ARGS) $(OBJECT_BENDER_DOC) \
	/win32icon:$(ICON) $(addprefix /resource:, $(OBJECT_BENDER_RESOURCE))

################
# ObjectViewer #
################

OBJECT_VIEWER_ROOT     := source/ObjectViewer
OBJECT_VIEWER_FOLDERS  := . Parsers Properties System
OBJECT_VIEWER_FOLDERS  := $(addprefix $(OBJECT_VIEWER_ROOT)/, $(OBJECT_VIEWER_FOLDERS))
OBJECT_VIEWER_SRC      := $(foreach sdir, $(OBJECT_VIEWER_FOLDERS), $(wildcard $(sdir)/*.cs))
OBJECT_VIEWER_DOC      := $(addprefix /doc:, $(foreach sdir, $(OBJECT_VIEWER_FOLDERS), $(wildcard $(sdir)/*.xml)))
OBJECT_VIEWER_RESX     := $(foreach sdir, $(OBJECT_VIEWER_FOLDERS), $(wildcard $(sdir)/*.resx))
OBJECT_VIEWER_RESOURCE := $(addprefix $(OBJECT_VIEWER_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(OBJECT_VIEWER_RESX)))))
OBJECT_VIEWER_OUT       = $(OUTPUT_DIR)/ObjectViewer.exe

$(call create_resource, $(OBJECT_VIEWER_RESOURCE), $(OBJECT_VIEWER_RESX))

$(OBJECT_VIEWER_OUT): $(OUTPUT_DIR)/OpenBveApi.dll $(OBJECT_VIEWER_SRC) $(OBJECT_VIEWER_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(OBJECT_VIEWER_OUT)$(COLOR_END)
	@$(CSC) /out:$(OBJECT_VIEWER_OUT) /target:winexe /main:OpenBve.Program $(OBJECT_VIEWER_SRC) $(ARGS) $(OBJECT_VIEWER_DOC) \
	/reference:$(OPEN_BVE_API_OUT) /reference:$(OUTPUT_DIR)/OpenTK.dll \
	/win32icon:$(ICON) $(addprefix /resource:, $(OBJECT_VIEWER_RESOURCE))

###############
# TrainEditor #
###############

TRAIN_EDITOR_ROOT     := source/TrainEditor
TRAIN_EDITOR_FOLDERS  := . CsvB3dDecoder Properties TrainsimApi/Codecs TrainsimApi/Geometry TrainsimApi/Platform TrainsimApi/Vectors
TRAIN_EDITOR_FOLDERS  := $(addprefix $(TRAIN_EDITOR_ROOT)/, $(TRAIN_EDITOR_FOLDERS))
TRAIN_EDITOR_SRC      := $(foreach sdir, $(TRAIN_EDITOR_FOLDERS), $(wildcard $(sdir)/*.cs))
TRAIN_EDITOR_DOC      := $(addprefix /doc:, $(foreach sdir, $(TRAIN_EDITOR_FOLDERS), $(wildcard $(sdir)/*.xml)))
TRAIN_EDITOR_RESX     := $(foreach sdir, $(TRAIN_EDITOR_FOLDERS), $(wildcard $(sdir)/*.resx))
TRAIN_EDITOR_RESOURCE := $(addprefix $(TRAIN_EDITOR_ROOT)/, $(subst /,., $(subst /./,/, $(patsubst source/%.resx, %.resources, $(TRAIN_EDITOR_RESX)))))
TRAIN_EDITOR_OUT       = $(OUTPUT_DIR)/TrainEditor.exe

$(call create_resource, $(TRAIN_EDITOR_RESOURCE), $(TRAIN_EDITOR_RESX))

$(TRAIN_EDITOR_OUT): $(TRAIN_EDITOR_SRC) $(TRAIN_EDITOR_RESOURCE)
	@echo $(COLOR_MAGENTA)Building $(COLOR_CYAN)$(TRAIN_EDITOR_OUT)$(COLOR_END)
	@$(CSC) /out:$(TRAIN_EDITOR_OUT) /target:winexe /main:TrainEditor.Program $(TRAIN_EDITOR_SRC) $(ARGS) $(TRAIN_EDITOR_DOC) \
	/win32icon:$(ICON) $(addprefix /resource:, $(TRAIN_EDITOR_RESOURCE))