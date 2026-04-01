# AUR Release

The `-git` package should handle itself in the CI pipeline.

1. Update the version in `scpdiscord.pkgbuild` and in the `.csproj` files.
2. Delete the existing `PKGBUILD` in the AUR repo.
3. Copy the `scpdiscord.pkgbuild` from this repo to the AUR repo and name it `PKGBUILD`.
4. Copy the `scpdiscord.install` file from this repo to the AUR repo if changed.
5. Delete any old directories in the AUR repo dir.
6. Run `makepkg --nobuild && makepkg --printsrcinfo > .SRCINFO` in the AUR repo.
7. If the diff looks good `git commit -m "<version number>"`.
8. Double check `git log -p` looks good and then `git push`.