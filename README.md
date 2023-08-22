# SharpSenz

**SharpSenz** is a C# intsrumentation library for decoupling reactive code from the main application code interactively as you type your application code in.

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
(This step is a temporary workaround for the promlem we don't know how to solve)
After installing the SharpSenz NuGet Package you need to comment out two XML lines as specified in the image below.

![Hyperspace project](Assets/UsageExample/02_AddingReference.png)

Now we can begin instrumenting our code

### 3. Adding the [SignalsSource] attribute
In our hyperspace example we want the Hyperdrive class to publish few signals.
Later we could implement this signals as log messages, performance counters or anything else.

First thing we need to do is to mark Hyperdrive class with the [SignalSource] attribute.

![Hyperspace project](Assets/UsageExample/03_AddingAttribute.png)

After adding the appropriate "using" statement, the entire Hyperdrive class got few compilation errors from SharpSenz Roslyn analyzers.
Please don't be afraid, we will immediatelly fix them with the SharpSenz Roslyn Code Fixes.

![Hyperspace project](Assets/UsageExample/04_AddingUsing.png)

### 4. Making class Partial

The first fix is to make Hyperdrive class partial since the generated code resides in a separate file.

![Hyperspace project](Assets/UsageExample/05_MakingClassPartial.png)

### 5. Adding the SignalsMultiplex class and member

Next we will add an inner class for spreading the signals to all registered signals receptors.
We call this class "Multiplex".

![Hyperspace project](Assets/UsageExample/06_AddingMultiplexClass.png)

And we will add a new instance of our Multiplex class.

![Hyperspace project](Assets/UsageExample/07_AddingMultiplexMember.png)

With both class and its instance defined the code looks like in the image below.

![Hyperspace project](Assets/UsageExample/08_BeforeAddingSignals.png)

### 6. Adding the signal to the existing method

It's time to add the actual signals to our code.
We recommend to add the signal staring with its message string.

Just add the comment starting with the "SIG:" prefix and put your message afterwards.

![Hyperspace project](Assets/UsageExample/09_AddingSignals.png)

Each signal comment needs a corresponding multiplex method call.

Apply the appropriate code fix to automatically get the multiplex method name containing both calling method name and the message.

![Hyperspace project](Assets/UsageExample/10_GeneratingFunctionCalls.png)

After all the multiplex method calls were added the code looks like in the image below.

![Hyperspace project](Assets/UsageExample/11_SignalMethodsCallsAdded.png)

**Wait, but it compiles!?!**

If you compile this code right now, it will surprisingly be compiled successfully.
This happens thanks to the automatic roslyn code genegation of the Multiplex class.
We will observe the automatically generated code in a moment.

**This automatic code generation is the main purpose of the SharpSenz project**

You can add parameters to the multiplex method calls.
Code generation will take these parameters into consideration automatically.

Just type the parameters into the method call itself!

![Hyperspace project](Assets/UsageExample/12_TypingParametersIn.png)

After adding parameters to the multiplex method calls the code looks like in the image below.

![Hyperspace project](Assets/UsageExample/13_ResultingCodeWithSignals.png)

Now let's take a look at the generated code.

### 7. Observing the generated SignalsMultiplex and ISignalsReceptor classes

As you can see in the image below each signal gets its own method with the correspondings parameters.

Signal context contains all the information about where the signal comes from.
All this information is constant value, so no perfromance penalties here.

![Hyperspace project](Assets/UsageExample/14_ObservingGeneratedMultiplex.png)

Multiplex responsibility is to catch the correct signal context and call all the ISignalsReceptor implementations.

ISignalsReceptor definition is pretty straightforward.

![Hyperspace project](Assets/UsageExample/15_ObservingGeneratedSignalsReceptor.png)

### 8. Observing main

Now let's take a look at the main module of the system.

Main remains unchanged.
And there is some power in this statement, since there is no dependency on any specific logging framework.
The code still compiles and run successfully without dependencies.

![Hyperspace project](Assets/UsageExample/16_ObservingMain.png)

This makes Unit Testing much easier.

### 9. Unit Testing the component without any dependency

As you can see in the image below Unit Testing is possible without any dependency.

![Hyperspace project](Assets/UsageExample/17_ObservingUnitTestsNoDependencies.png)

### 10. Developing the console logging

Let's develop some implementations of ISignalsReceptors.
We can start with the simplest logging to the Console. 

We recommend to implement ISignalsReceptor interface explicitly.
This way everytime the interface changes, you will not miss the methods that are not needed anymore.

You can also find examples of signal context usage in the implementation below.

![Hyperspace project](Assets/UsageExample/18_ConsoleLoggerImplementation.png)

The usage is straightforward.

![Hyperspace project](Assets/UsageExample/19_ConsoleLoggerUsage.png)

### 11. Adding NLog logging instance

The beauty of the decoupling is that we can have multiple implementations of the logging mechanism.
This implementations can live in our code base at the same time and could be changed even at runtime.
This is not the case with the traditional approach to logging.

Let's add an implementation that calls to NLog.

And the fact the Hyperspace project itself remains untouched is especially satisfying.

![Hyperspace project](Assets/UsageExample/20_NLogImplementation.png)

Let's have both loggers to work in parallel just make a point they can.

![Hyperspace project](Assets/UsageExample/21_NLogUsage.png)

### 12. Adding Performance Counters and MessageBus instances

Later we can decide to add Performance Counters implementation or some message bus logic.

Again the Hyperspace project remains untouched.

![Hyperspace project](Assets/UsageExample/22_PerfCountersAndMessageBus.png)

Hope you've enjoyed this walkthrough.
