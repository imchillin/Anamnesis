git submodule update --init --recursive
git submodule foreach --recursive git reset --hard
git submodule foreach git pull origin master
git submodule foreach git pull origin main
PAUSE