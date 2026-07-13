

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


https://learn.microsoft.com/en-us/visualstudio/debugger/create-custom-visualizers-of-data?view=visualstudio


https://learn.microsoft.com/en-us/visualstudio/debugger/walkthrough-writing-a-visualizer-in-csharp?view=visualstudio


https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/debugger-visualizer/debugger-visualizers?view=visualstudio



Notes about State FrameWork

1. The active state must allow exit. It must offer a CanExit method.
activeState.CanExit(). Also see CanEnter()
Need to study more

2. Need to know if a transition is Back. Study the following class ImplicitTargetNames.
It has three back, forceBack, backToPreviousParent. Need to understand all of the three.

3. Need to understand PriorStateStack, PriorStateStackElement. Also there is ignoreBackEnter which needs to be understood.

4. Search for the following.
	// If activeState is parallel state and toState is outside the parallel state
	// then, exit the parallel state
	// otherwise, exit on the sub-state of parallel state
	// Else exit the active state

5. Fully understand the concept of ActiveSubstate in ParallelStateBase.

6. What is TransitionAllowed in StateBase. There are some states to which transitioins are not allowed. If you search for "transitionAllowed" in state xml files, you will find that almost all of them are named as InActive. For example, Live.InActive, 

```xml
<state name="InactiveState" transitionallowed="false">
</state>
```

or 

```xml
<state name="InActive" transitionallowed="false">
</state>
```

7. Find the following line in state machine base. What is background state machine, back behavior, and _backTransitionSwitch
// For background state machine, back behavior is not allowed, _backTransitionSwitch is always false

8. What are the following in StateBase class

```cs
public string UnstackByParent
{
	get { return _unstackByParent; }
	set { _unstackByParent = value; }
}

public string UnstackByState
{
	get { return _unstackByState; }
	set { _unstackByState = value; }
}
```
You can see the unstackByState in the following Live state file as well.
```xml
<state name="Live_Live" IsRootState="true" unstackByState="Live.Live_Live">
</state>
```
and unstackByParent
```xml
<state name="SplitCursorPlacementLeftState" unstackByParent="MeasurementOnAnalytics" base="CursorPlacementState">
</state>
```

9. What is PopOnce on StateBase and TransitionBase. Find the following in StateMachineBase.

```cs
popState.PopOnce = transition.PopOnce;
```

10. What is IsRootState?
Is this state the root of navigation, the one state that "back" will lead you to. Live.Live_Live is the only one RootState in the app.
Note this is not the first state the app will acquire. For example there are StartupWizard, 
If this is set, then the state history is cleared when this state is entered. All state history up to this point is forgotten.
Set this via the state table XML. Don't manipulate this property from a subclass. This must be a readonly. 

11. What is PowerSaveTrigger? I think this trigger is fired after a long sleep time. Need to find more. To debug you can do the following.
Live -> Image Softbutton -> ImageMenuState -> ImageOverlayState -> OverlayOpacityState.
Now place a break point at the start of PerformTriggeredTransition in StateMachineBase class. Wait for a long time, and press F5. You should see the same break point hit again and with the trigger PowerSaveTrigger

12. What is the Infrastructure.StateManager.StateList class? Related is the method GetActiveState() in the StateMachineBase class.  

13. What are parent states?
If you look at AppStateTable1.xml and AppStateTable2.xml, you see ParentStates. For example, Live, Profiles and StartupWizard in AppStateTable1.xml. Then there are SubStates to these parent states. For example Live has its substates defined in assembly="Modules.Live" and xmltablename="LiveStateTable.xml". So there is a two step hierarchy, Parent and Substates. 
Each Parent state has its set of Transitions and triggers as well. 
Finally there are parent states without any transitions. Need to find out more about them. Example, the state StartupWizard.

14. AppStateTable2.xml has globaltransitions. Not sure what they are. Need to find out.

15. And finally there are parallel states defined in the AppStateTable2.xml file.

16. Do the following. Run the App -> Live -> FF. Now place a break point at the following line in PerformTriggeredTransition method of StateMachineBase. 

```cs
TransitionBase transition = activeState.GetTransitionForTrigger(trigger);
```

Get inside of the method GetTransitionForTrigger. The trigger name is BackPress and Source is HardKey. Once inside of this method, 

```cs
foreach (TransitionBase transition in _transitions)
{
	if (transition.Trigger.TriggerName.Equals(trigger.TriggerName) && transition.Trigger.Source == trigger.Source)
	{
		transitionFound = transition;
		break;
	}
}
```

The _transitions represents the transitions for the state current state which is ActiveState. The total count is 47. How come there are so many states transitions?

17. What is backenter="false" in a state tag of an xml file like the following 

```xml
<state name="LiveFreezeFrameState" backenter="false">
  ...
</state>
```

Do the following App -> Live -> FF. Now place a break point at the following line in PerformTriggeredTransition method of StateMachineBase.
```cs
switch (transition.TargetName)
{
	case ImplicitTargetNames.Back:
		element = _priorStateStack.GetPriorState(currentState); // <-- Place break point on this line
		break;
}
```
Observe _priorStateStack has three elements. Live.Live_Live, Live.ImageCapture, FreezeFrame.ActiveState
Press F11 and get into the method GetPriorState, and do a step by step debug. By the end, 
Notice how the method recursively calls itself to compleately unwound. And reaches back to Live.Live_Live.
This is because both the top two states Live.ImageCapture, FreezeFrame.ActiveState have backenter = false.
This means those states cannot be acquired using back as target. So the final state here is Live.Live_Live.
Notice ImageCapture also has backenter="false" 
```xml
<state name="ImageCapture" backenter="false">
``` 

18. 


