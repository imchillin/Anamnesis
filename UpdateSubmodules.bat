git submodule foreach --recursive git reset --hard
git submodule foreach git pull
git submodule update --init --recursive
PAUSE