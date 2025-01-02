#!/usr/bin/env bash

# Basic shell script to retry creating DMG as Azure is buggy
# https://github.com/actions/runner-images/issues/7522

echo "Attempting to create Apple DMG"
for i in {1..10}; do
	if /usr/bin/sudo /usr/bin/hdiutil create macbuild.dmg -volname "OpenBVE" -fs HFS+ -srcfolder "mac/OpenBVE.app"; then
		echo "Successfully created DMG on attempt $i"
		break
	elif [[ $i -eq 10 ]]; then
		echo "Failed to create DMG after $i attempts"
		exit 1
	else
		echo "Failed to create DMG on attempt $i, retrying"
	fi
done