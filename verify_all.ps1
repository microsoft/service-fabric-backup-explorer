git add -A .
git clean -xdf
./build_all.ps1 -build
./run_unittests.ps1
