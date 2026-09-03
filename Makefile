NAME     := redungeon
BASE_APK := $(NAME).apk
KEYSTORE ?= $(HOME)/test.keystore
KS_PASS  ?= pass:password
PACKAGE  := com.nitrome.redungeon

.PHONY: build ship install launch log clean

build:
	dotnet build src -c Release

ship: build
	mkdir -p build/assemblies
	cp src/bin/Release/KnighterAndroid.dll build/assemblies/KnighterAndroid.dll
	cp $(BASE_APK) build/$(NAME)_mod.apk
	cd build && zip -0 $(NAME)_mod.apk assemblies/KnighterAndroid.dll
	zipalign -f -P 16 4 build/$(NAME)_mod.apk build/$(NAME)_mod_aligned.apk
	apksigner sign --ks $(KEYSTORE) --ks-pass $(KS_PASS) --out build/$(NAME)_mod_ready.apk build/$(NAME)_mod_aligned.apk
	@echo "=== ready: build/$(NAME)_mod_ready.apk ==="

install: ship
	adb install -r build/$(NAME)_mod_ready.apk

launch:
	adb shell monkey -p $(PACKAGE) -c android.intent.category.LAUNCHER 1

log:
	adb logcat -c
	adb logcat | grep -iE "monodroid|AndroidRuntime|$(NAME)"

clean:
	rm -f build/$(NAME)_mod.apk build/$(NAME)_mod_aligned.apk build/$(NAME)_mod_ready.apk build/*.idsig