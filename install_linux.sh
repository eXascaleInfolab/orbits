sudo apt-get -y update
suod apt-get -y upgrade
sudo apt-get -y install build-essential cmake libopenmpi-dev libopenblas-dev liblapack-dev libarmadillo-dev
sudo apt-get -y install r-base gnuplot
sudo apt-get -y install python-dev
curl "https://bootstrap.pypa.io/pip/2.7/get-pip.py" -o get-pip.py
sudo python get-pip.py
rm get-pip.py
pip2 install numpy scipy pandas sklearn
pip2 install tensorflow==1.14.0
python linux_build.py
