# Running simulations on Dev Host

## Motivation
To objective is to be able to run a test (simulation) in the context of a database (so for instance, this can be used to put the DB in a certain state for doing front-end development), which can be a real time-saver during development.

## Usage Instructions     

1. Start DevHost
2. To see what simulations are available, **GET** the endpoint **http(s)://\<host\>:\<port\>/simulation/list**. To narrow down the simulations, use the *assembly*, *module*, and/or *simulation* query string parameters. For example, **http(s)://\<host\>:\<port\>/simulation/list?module=Session Tests** (or) **http(s)://\<host\>:\<port\>/simulation/list?module=Dish Offers Tests&simulation=Dish offers are generated every day**
3. Once you're satisfied with the set of simulations returned by step 2 and you want to execute them, change list to execute in the URL

  
## WARNINGS    

1. Moving time forward DOES NOT work (yet). The call won't crash, but the time won't be moved forward either, which means timers will not be fired. This will be addressed separately in the future.
2. By default, if you do `do! Ecosystem.interceptConnector ...` that connector interception WILL CONTINUE to exist after the simulation is complete. To resolve this, do `use! __ = Ecosystem.useConnector ...`; this will un-register the interceptor after the simulation is complete.
3. While a simulation is running, if you attempt to run other code that uses an interceptor that is being simulated, that code will also use that simulator's interceptor, leading to unexpected behavior.
4. If multiple simulations are running in parallel, each with their own interceptors, they will cross-fire and lead to unpredictable behavior. Long story short, the current connector interception is global, so running multiple simulations in parallel is probably going to screw things up.

