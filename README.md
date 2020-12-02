# Overview

<img img align="right" width="50" height="50" src="orbits_logo.png" >

- **Paper**: Mourad Khayati, Ines Arous, Zakhar Tymchenko and Philippe Cudré-Mauroux: [ORBITS: Online Recovery of Missing Values in Multiple Time Series Streams](http://vldb.org/pvldb/vol14/p294-khayati.pdf). PVLDB 2021.
- **Algorithms**: The benchmark evaluates all the algorithms mentioned in the paper: ORBITS, SPIRIT, GROUSE, OGDImpute, pcaMME, TKCM and M-RNN<sup>*</sup>. To enable/disable any algorithm, please refer to the *Algorithms customization* section below.
- **Datasets**: The benchmark evaluates all the datasets used in the paper: gas (drfit10), motion, bafu and soccer<sup>*</sup>. To enable/disable any dataset, please refer to the *Datasets customization* section below.
- **Scenarios**: The benchmark will execute the full set of 11 recovery scenarios and report the error using RMSE, MSE and MAE. 
A detailed description of the recovery scenarios can be found [here](https://github.com/eXascaleInfolab/orbits/blob/master/TestingFramework/README.md).

<sup>*</sup>disabled by default as it takes a couple of days to run.
<!--
The online scenarios are described [here](https://github.com/eXascaleInfolab/orbits/blob/master/TestingFramework/bin/Debug/results/plotfiles/streaming_end.txt) while the batch scenarios are described [here](https://github.com/eXascaleInfolab/orbits/blob/master/TestingFramework/bin/Debug/results/plotfiles/batch_mid.txt). 
-->

[**Prerequisites**](#prerequisites) | [**Build**](#build) | [**Execution**](#execution) | [**Benchmark Customization**](#benchmark-customization) | [**Citation**](#citation)
___ 
 
## Prerequisites

- Ubuntu 16 or 18 (including Ubuntu derivatives, e.g., Xubuntu).
- Clone this repository.
- Mono: Install mono from https://www.mono-project.com/download/stable/ (takes few minutes)

___
## Build

- Build the Testing Framework using the installation script located in the root folder (takes few minutes):
```bash
    $ sh install_linux.sh
```
___
## Execution


```bash
    $ cd TestingFramework/bin/Debug/
    $ mono TestingFramework.exe 
```

- **Results**: All results will be added to `Results` folder. The accuracy results of all algorithms will be sequentially added for each scenario and dataset to: `Results/.../.../.../error/`. The runtime results of all algorithms will be added to: `Results/.../.../.../runtime/`. The plots of the recovered blocks will be added to the folder `Results/.../.../.../plots/`.

- **Warning**: The test suite with the default setup will take ~20 hours to finish. 

___
## Benchmark Customization 

### Algorithms customization

To enable an additional algorithm
- open the file `TestingFramework/config.cfg`
- add the name of the algorithm to the line `EnabledAlgorithms =`


### Datasets customization

- All the datasets used in this paper can be found in: `TestingFramework/bin/Debug/data/`
- To enable an additional dataset
  - open the file `TestingFramework/config.cfg`
  - Add the name of the dataset to the line `Datasets =`

- To add a new dataset to the benchmark
  - import the file to `TestingFramework/bin/Debug/data/{name}/{name}_normal.txt` (`name` is the name of your data).
  - Requirements: rows>= 1'000; columns>= 10; column separator = space; row separator = newline


### Scenario customization

To enable an additional recovery scenario
- open the file `TestingFramework/config.cfg`
- add the name of the scenario to the line `Scenarios =`


___
## Citation
```bibtex
@inproceedings{orbits2021vldb,
 author    = {Mourad Khayati and Ines Arous and Zakhar Tymchenko and Philippe Cudr{\'{e}}{-}Mauroux},
 title     = {ORBITS: Online Recovery of Missing Values in Multiple Time Series Streams},
 booktitle = {Proceedings of the VLDB Endowment},
 volume    = {14},
 number    = {3},
 year      = {2021}
}
```


<!--
___
___
## Prerequisites and dependencies (macOS) 

- The benchmark runs also on macOS, but takes much longer than Linux. 
- macOS 10.13 or higher, homebrew
- Clone the current repository
```bash
    $ xcode-select --install
    $ git clone https://github.com/eXascaleInfolab/orbits.git
```
- Mono: Install mono from https://www.mono-project.com/download/stable/ and restart the terminal window.

- If you are running macOS 10.14, you need to install C/C++ headers using the following command:
```bash
    $ open /Library/Developer/CommandLineTools/Packages/macOS_SDK_headers_for_macOS_10.14.pkg
```

## Build 

- Build all the algorithms and Testing Framework using the installation script located in the root folder:
```bash
    $ sh install_mac.sh
```
## Execution

```bash
    $ cd TestingFramework/bin/Debug/
    $ mono TestingFramework.exe
```

## Benchmark customization

The algorithm and dataset customization is identical to Linux (see above).

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
- the line starting with `IgnoreAlgorithms =` provides codes for all the algorithms in the benchmark

-->

