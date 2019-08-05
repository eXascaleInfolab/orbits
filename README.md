# InCD_benchmark

#### Repository structure
- Algorithms - missing value recovery algorithms: CDRec, InCD, STMVL, TRMF, TKCM, SPIRIT, TeNMF, GROUSE (SAGE as an online variant), OGDImpute, SSA, M-RNN, DynaMMo, MD_ISVD.
- Datasets - different datasets and time series from different sources.
- Testing Framework - a program to run automated suite of tests on the datasets with the algorithms mentioned above.

### Prerequisites and dependencies (Linux)

- Ubuntu 16 and higher (or Ubuntu derivatives like Xubuntu)
- Sudo rights on the user
- Clone the repository
```bash
    $ git clone https://github.com/eXascaleInfolab/InCD_bench-19.git
```
- C/C++ compilers and linear algebra libraries:
```bash
    $ sudo apt-get install build-essential cmake libopenmpi-dev libopenblas-dev liblapack-dev libarmadillo-dev libmlpack-dev
```
- GNU Octave with C++ interop libraries, R to enable calculation of errors (MSE/RMSE, correlation), Gnuplot to enable recovery visualization and MSE plots:
```bash
    $ sudo apt-get install octave-pkg-dev r-base gnuplot
```
- Mono Runtime and Compiler: follow step 1 from the installation guide in https://www.mono-project.com/download/stable/ for your Ubuntu version and afterwards do:
```bash
    $ sudo apt-get install mono-devel
```
- Python and python libraries:
```bash
    $ sudo apt install python-dev python-pip
    $ pip2 install numpy scipy pandas sklearn
    $ pip2 install tensorflow
```

#### Build & tests

- Restart the terminal window after all the dependencies are installed. Open it in the root folder of the repository.
- Build all the algorithms and Testing Framework using a script in the root folder (takes around 1 minute):
```bash
    $ python linux_build.py
```
- Run the benchmark:
```bash
    $ cd TestingFramework/bin/Debug/
    $ mono TestingFramework.exe
```
- Test suite will go over datasets one by one and executes all the scenarios for them with both precision test and runtime test. Plots folder in the root of the repository will be populated with the results.
- Remark: full test suite with the default setup will take a sizeable amount of time to run (around 1 day depending on the hardware) and will produce up to 3GB of output files with all recovered data and plots unless stopped early.

#### Customize datasets

To add a dataset to the benchmark
- import the file to `TestingFramework/bin/Debug/data/{name}/{name}_normal.txt`
- - Requirements: >= 10 columns, >= 1'000 rows, column separator - empty space, row separator - newline
- add `{name}` to the list of datasets in `TestingFramework/config.cfg`

#### Customize algorithms

To exclude an algorithm from the benchmark
- open the file `TestingFramework/config.cfg`
- add an entry `IgnoreAlgorithms =` and specify the list of algorithm codes to exclude them
- the line starting with `#IgnoreAlgorithms =` provides codes for all the algorithms in the benchmark


### Prerequisites and dependencies (macOS) -- Experimental

- It's highly recommended to run the benchmark on linux, GUI is not necessary, everuthing is CLI-only. Output files with plots can be just copied afterwards to be viewed on any machine. If you don't have a possibility to run the benchmark on Linux, it's possible to make it work on macOS with a few caveats:
- - TRMF algorithm doesn't work with octave on macOS, so it will be disabled.
- - The installation can take a really long time. The longest processes are installation of LLVM with brew and compilation of mlpack since brew doesn't contain this one.
- macOS 10.13 or higher, homebrew
- Sudo rights on the user
- Clone the repository
```bash
    $ xcode-select --install
    $ git clone https://github.com/eXascaleInfolab/bench-vldb19.git
```
- C/C++ compilers and linear algebra libraries:
```bash
    $ brew update
    $ brew install --force-bottle llvm
    $ brew install cmake openblas lapack armadillo boost
```
- If you're running macOS 10.14 you also have to install C/C++ headers by typing the command below and going through the installation screen:
```bash
    $ open /Library/Developer/CommandLineTools/Packages/macOS_SDK_headers_for_macOS_10.14.pkg
```
- R to enable calculation of errors (MSE/RMSE, correlation), Gnuplot to enable recovery visualization and MSE plots:
```bash
    $ brew install R gnuplot
```
- Mono Runtime and Compiler: Install the package provided by Mono in https://www.mono-project.com/download/stable/
- Python and python libraries:
```bash
    $ brew install python2
    $ pip2 install numpy scipy pandas sklearn tensorflow
```

#### Build & tests

- Restart the terminal window after all the dependencies are installed. Open it in the root folder of the repository.
- Build all the algorithms and Testing Framework using a script in the root folder (takes around 1 minute):
```bash
    $ python mac_build.py
```
- Run the benchmark:
```bash
    $ cd TestingFramework/bin/Debug/
    $ mono TestingFramework.exe
```

#### Customize datasets and algorithms

The process is identical to Linux.
