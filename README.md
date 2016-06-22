# wip-vs-leadtime
Monte carlo simulation of wip vs leadtime.

This small tool demostrates the relation between wip limits and bottlenecks in a project.

On the second tabpage of the tool you can play with the parameters of the simulation.
The configuration allows you to modify the (swim)lanes, add or remove teammembers, change wip limits etc.
By default a sample configuration is loaded using 4 swimlanes, 4 dev-members, 2 test-members and 10 tasks with random tasksizes. The wip limits on the lanes are set to (backlog)infinite, (dev)1, (test)1, (done)infinite.

The tool accepts a Json file as its configuration. 
So it should be fearly easy to store and load your specific configurations. 

If you have any questions or suggestions please let me know.
