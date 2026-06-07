

## About the repo

1. Some of the projects that will come up are as follows.

Avak.StateMachine.Core
Avak.StateMachine.Console
Avak.StateMachine.UI
Avak.StateMachine.Vs.Extns
Avak.StateMachine.Vs.Extns

AVAK stands for 

Aaaryavart Vaigyanik Anusandhan Kendr

2. Avak.StateMachine.Old will eventually be removed.

3. State file

```xml
<?xml version="1.0" encoding="utf-8" ?>
<AvakStates Namespace="Avak.StateMachine.Sample.ConsoleUI">
	<Triggers>
		<Trigger Name="EnterBbFromAa" Source="Event"/>
		<Trigger Name="EnterCcFromAa" Source="Event"/>
		<Trigger Name="EnterBbFromCc" Source="Event"/>
	</Triggers>
	<States Initial="Aa"  >
		<State Name="Aa" >
			<Transition Trigger="EnterBbFromAa" Target="Bb"/>
			<Transition Trigger="EnterCcFromAa" Target="Cc"/>
		</State>
		<State Name="Bb" Namespace="Avak.StateMachine.Sample.ConsoleUI.StateManager.States" ></State>
		<State Name="Cc" Namespace="Avak.StateMachine.Sample.ConsoleUI.StateManager">
			<Transition Trigger="EnterBbFromCc" Target="Bb"/>
		</State>
	</States>
</AvakStates>
```

Triggers and Transitions will not have corresponding namespaces, because the user will not create specific classes for this elements. Generic classes will be made available in the framework.

On the other hand, for states, framework will define a base class, and the users will define their own state classes by deriving from the base class. The namespace corrospnding to the state elements in the xml file will determine where those classes will be defined.


https://learn.microsoft.com/en-us/visualstudio/debugger/create-custom-visualizers-of-data
?view=visualstudio




