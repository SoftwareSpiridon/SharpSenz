# SharpSenz

**SharpSenz** is a C# intsrumentation library for decoupling reactive code from the main application code.

**SharpSenz enables:**
- Abstracting logging / traces / monitoring away
- Unit tests simplification
- Being able to decide on a specific logging frameworks later
- Completely replace logging framework or have many of them at any stage of the project
- Implement additional reactive logic like performance counters collection or message bus communication without touching the main application code

## Overview

SharpSenz utilizes Roslyn compiler capabilities to generate abstract interface calls as you type your main application code in.

The implementations of the generated interface may do whatever is practical: logging / updating performance counters / cross-services communication / anything that comes to your mind later.

It allows the developer not to take dependencies on any monitoring library during the early stages of the project and keep the architecture clean.

It allows to delay the decision about monitoring framework to the latest responsible moment as well as have multiple monitoring implementations at the same time without changing the original code.

It also makes the unit / component testing easier by making all the reactive code optional.

## Walkthrough

The below explanation looks long, but the actual work is really fast as all you need to do is to press "Alt + Enter" few times.

### 1. Example class without logging
Take a look at this siimple project. Hyperspace is the component we will apply an instrumentation to.
We can think about it as one of the components of the Spaceship.
Spaceship is the main program which calls to the Hyperdrive class of the Hyperspace project.
Next we will apply instrumentation to the JumpThroughHyperspace method of the Hyperdrive class.

![Hyperspace project](Assets/UsageExample/01_CleanProject.png)

### 2. Installing the SharpSenz NuGet package

![Hyperspace project](Assets/UsageExample/02_AddingReference.png)

### 3. Adding the [SignalsSource] attribute

![Hyperspace project](Assets/UsageExample/03_AddingAttribute.png)

![Hyperspace project](Assets/UsageExample/04_AddingUsing.png)

### 4. Making class Partial

![Hyperspace project](Assets/UsageExample/05_MakingClassPartial.png)

### 5. Adding the SignalsMultiplex class and member

![Hyperspace project](Assets/UsageExample/06_AddingMultiplexClass.png)

![Hyperspace project](Assets/UsageExample/07_AddingMultiplexMember.png)

![Hyperspace project](Assets/UsageExample/08_BeforeAddingSignals.png)

### 6. Adding the signal to the existing method

![Hyperspace project](Assets/UsageExample/09_AddingSignals.png)

![Hyperspace project](Assets/UsageExample/10_GeneratingFunctionCalls.png)

![Hyperspace project](Assets/UsageExample/11_SignalMethodsCallsAdded.png)

![Hyperspace project](Assets/UsageExample/12_TypingParametersIn.png)

![Hyperspace project](Assets/UsageExample/13_ResultingCodeWithSignals.png)

### 7. Observing the generated SignalsMultiplex and ISignalsReceptor classes

![Hyperspace project](Assets/UsageExample/14_ObservingGeneratedMultiplex.png)

![Hyperspace project](Assets/UsageExample/15_ObservingGeneratedSignalsReceptor.png)

### 8. Observing main

![Hyperspace project](Assets/UsageExample/16_ObservingMain.png)

### 9. Unit Testing the component without any dependency

![Hyperspace project](Assets/UsageExample/17_ObservingUnitTestsNoDependencies.png)

### 10. Developing the console logging

Taking an advantage of the explicit interface implementation

![Hyperspace project](Assets/UsageExample/18_ConsoleLoggerImplementation.png)

Usage

![Hyperspace project](Assets/UsageExample/19_ConsoleLoggerUsage.png)

### 11. Adding NLog logging instance

Interface implementation

![Hyperspace project](Assets/UsageExample/20_NLogImplementation.png)

Adding a receptor instance

![Hyperspace project](Assets/UsageExample/21_NLogUsage.png)

### 12. Adding Performance Counters and MessageBus instances

![Hyperspace project](Assets/UsageExample/22_PerfCountersAndMessageBus.png)
