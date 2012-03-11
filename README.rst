NAntScript
==========

NAntScript provides custom tasks that allow you to script other custom tasks
using regular NAnt script.

How to use NAntScript tasks in NAnt?
------------------------------------
In order to use NAntScript tasks in NAnt, you can use one of the following
procedures:

1. Use the <loadtasks> task in your build file(s) to tell NAnt which assembly to
scan for tasks

For example:

  <loadtasks assembly="c:/broloco.NAntScript.1.0.0.0.net-2.0/bin/broloco.NAntTasks.dll" />


2. Copy the content of the NAntScript bin directory to the NAnt directory

In order to make certain tasks available to every build file, you can copy the
tasks assembly and all its non-assembly dependencies to the "<nant>\bin\tasks"
directory.

3. Modify NAnt configuration file (not recommended)

An <include> element can be added to the <task-assemblies> node in the
<framework> node for the .NET Framework version that you intend to use. The
"name" attribute of the <include> element should hold the absolute path to
broloco.NAntTasks.dll.

For example:

    <framework
            name="net-2.0"
                family="net"
                version="2.0"
                description="Microsoft .NET Framework 2.0"
        ....
    >
            <task-assemblies>
            ...
            <include name="c:/broloco.NAntScript.1.0.0.0.net-2.0/bin/broloco.NAntTasks.dll" />
            ...
                </task-assemblies>
        ....
    </framework>

Note: The NAnt configuration file (NAnt.exe.config) is considered internal, and
might change without being noticed.

How to build
------------
1.  Open command prompt in the NAntScript folder type 'nant'

That's it!

To create a release, type 'nant export'.

