# SharpSenz

SharpSenz is a C# abstract monitoring intsrumentation library. It utilizes Roslyn compiler to generate abstract interface calls as you type your code in.
The implementations of the generated interface may do whatever is practical: logging / update performance counters / cross-services communication / anything that comes to your mind later.
It allows the developer not to take the dependencies on any other monitoring library during the early stages of the project and keep the architecture clean.
It also makes the unit / component testing easier.
