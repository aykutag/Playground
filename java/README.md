Java Enumerable
----

This is a project to implement .NET style enumerable functions on deferred lazy collections using Java.

The goal, to provide

- Map
- Filter
- FlatMap
- OrderBy
- Take
- TakeWhile
- Skip
- SkipWhile
- Iter
- Iteri

Functionality that can serve as a general base for functional programming with Iterables in Java (using JDK 8 with lambdas).

Example
---

Currently you can already do this using the following example:

```java
List<String> strings = asList("oooo", "ba", "baz", "booo");            
                                                                       
Enumerable<String> items = Enumerable.init(strings)            
                                     .orderBy(i -> i.length());
                                                                       
for(String x : items){                                                 
    System.out.println(x);                                             
}                                                                      
```

And stuff like

```java
List<Integer> items = Enumerable.init(strings)            
					            .orderBy(String::length)
					            .map(String::length)    
					            .filter(i -> i == 2)     
                                .toList();                
```

