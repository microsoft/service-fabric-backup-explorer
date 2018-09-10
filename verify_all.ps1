# Keep all changes
git add -A .
# Clean up
git clean -xdf

# build and run tests
./build_all.ps1 -build
./run_tests.ps1
