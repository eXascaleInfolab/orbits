# ORBITS benchmark 

Mourad Khayati, Ines Arous, Zakhar Tymchenko and Philippe Cudré-Mauroux: *ORBITS: Online Recovery of Missing Values in Multiple Time Series Streams*. Submitted to the Proceedings of the VLDB Endowment (**PVLDB 2021**)
<!---

#### Repository structure
- Algorithms - missing blocks recovery algorithms: ORBITS (incd), TKCM, SPIRIT, GROUSE, OGDImpute, SSA, M-RNN, pcaMME.
- Datasets - different datasets and time series from different sources.
- Testing Framework - a program to run automated suite of tests on the datasets with the algorithms mentioned above.
 --->

## Prerequisites and dependencies (Linux)

- Ubuntu 16 or higher (including Ubuntu derivatives, e.g., Xubuntu).
- Clone this repository.
- Mono: Install mono from https://www.mono-project.com/download/stable/ and restart the terminal window.
- All the additional dependencies will be installed using the build script.

## Build

- Build all the algorithms and Testing Framework using the installation script located in the root folder:
```bash
    $ sh install_linux.sh
```

## Execution


```bash
    $ cd TestingFramework/bin/Debug/
    $ mono TestingFramework.exe 
```


## Benchmark customization

### Algorithms customization

To include/exclude an algorithm in/from the benchmark
- open the file `TestingFramework/config.cfg`
- update the line `#IgnoreAlgorithms =` with the list of algorithms to exclude

### Datasets customization

To add a dataset to the benchmark
- import the file to `TestingFramework/bin/Debug/data/{name}/{name}_normal.txt` (`name` is the name of your data).
- Requirements: rows>= 1'000, columns>= 10, column separator: empty space, row separator: newli


<!--
# InCD_benchmark

#### Repository structure
- Algorithms - missing value recovery algorithms: ORBITS (incd), TKCM, SPIRIT, GROUSE, OGDImpute, SSA, M-RNN, pcaMME.
- Datasets - different datasets and time series from different sources.
- Testing Framework - a program to run automated suite of tests on the datasets with the algorithms mentioned above.

### Prerequisites and dependencies (Linux)

- Ubuntu 16 and higher (or Ubuntu derivatives like Xubuntu)
- Sudo rights on the user
- Clone the repository
```bash
    $ git clone https://github.com/eXascaleInfolab/InCD_bench-19.git
```
- Mono Runtime and Compiler: follow step 1 from the installation guide in https://www.mono-project.com/download/stable/ for your Ubuntu version and afterwards do:
```bash
    $ sudo apt-get install mono-devel
```
- All other prerequisites will be installed using a build script.

#### Build & tests

- Restart the terminal window after all the dependencies are installed. Open it in the root folder of the repository.
- Build all the algorithms and Testing Framework using a script in the root folder (takes up to 5 minutes depending which prerequisites are already installed in the system):
```bash
    $ sh install_linux.sh
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

-->

___

## Prerequisites and dependencies (macOS) -- Experimental

- The benchmark runs on macOS with a few caveats:
- - TRMF algorithm is disabled (it doesn't work under octave on macOS).
- - The installation takes longer than Linux.
- macOS 10.13 or higher, homebrew
- Sudo rights on the user
- Clone the repository
```bash
    $ xcode-select --install
    $ git clone https://github.com/eXascaleInfolab/bench-vldb19.git
```
- If you're running macOS 10.14 you also have to install C/C++ headers by typing the command below and going through the installation screen:
```bash
    $ open /Library/Developer/CommandLineTools/Packages/macOS_SDK_headers_for_macOS_10.14.pkg
```
- Mono Runtime and Compiler: Install the package provided by Mono in https://www.mono-project.com/download/stable/
- All other prerequisites will be installed using a build script.

### Build & tests

- Restart the terminal window after all the dependencies are installed. Open it in the root folder of the repository.
- Build all the algorithms and Testing Framework using a script in the root folder (takes up to 10-12 minutes depending which prerequisites are already installed in the system):
```bash
    $ sh install_mac.sh
```
- Run the benchmark:
```bash
    $ cd TestingFramework/bin/Debug/
    $ mono TestingFramework.exe
```

### Benchmark customization

The process is identical to Linux.
