# Include comman commands.
. "./common.ps1"

# Keep all changes
git add -A .
# Clean up
git clean -xdf

# build and run tests
./build_all.ps1 -buildAll
./run_tests.ps1
