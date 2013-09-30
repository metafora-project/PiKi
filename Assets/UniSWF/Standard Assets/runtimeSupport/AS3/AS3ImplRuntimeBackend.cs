using UnityEngine;
using UnityEngine.Flash;
using System.Collections;
using pumpkin.events;

#if UNITY_EDITOR
using System;
using System.Reflection;
#endif

#if UNITY_FLASH
public class AS3ImplRuntimeBackend : MonoBehaviour, IAS3EventDelegateBackend {

	void Awake () {
		EventDelegate.s_eventDelegateBackend = this;
	}
	
	/**
	 * Compare function ptrs.
	 * 
	 * Due to delegate wrapping in AS3 it is not possible to compare events in JS.	 
	 **/
	public bool compareToDelegate( EventDelegate delegateObj, EventDispatcher.EventCallback delegateCallback, bool useCapture ) {
	
#if UNITY_EDITOR
		if( delegateObj.delegateCallback == delegateCallback )
#else
		if( ActionScript.Expression<bool>( "({0} == {1})", delegateObj.delegateCallback, delegateCallback ) )
#endif			
		{
			return true;
		}
		
		
#if UNITY_EDITOR		
		// Fast check
		if( delegateObj.delegateCallback.GetHashCode() == delegateCallback.GetHashCode() && delegateObj.useCapture == useCapture ) {
			return true;
		}
		
		MulticastDelegate internalDelegate = cacheCompilerGenerated( delegateObj.delegateCallback );

		// Handle compilter genreated
		MulticastDelegate dOther = delegateCallback;			
		if( dOther != null && internalDelegate != null && dOther.Target != null ) {
			
			
			System.Object dTargetOther = (System.Object)dOther.Target;			
			

			FieldInfo[] attrsB = dTargetOther.GetType().GetFields( BindingFlags.NonPublic | BindingFlags.Instance );
									
			if( attrsB.Length > 0) {
				
				if(attrsB[0].Name[0] == '$') {
					
					// Get as deledages						
					MulticastDelegate B = attrsB[0].GetValue( dTargetOther ) as MulticastDelegate;
				
					// Compare delegates
					bool cmp = internalDelegate == B;
										
					Debug.LogWarning( "Event arg declared on " + B.Method.Name + ":" + B.Target + " must be CEvent, please refactor events for Flash player builds"  );
					
					// Return
					return cmp && delegateObj.useCapture == useCapture;
				}
			}
		}
		
		bool cmpB = delegateCallback == delegateObj.delegateCallback;

		// Default
		return cmpB && delegateObj.useCapture == useCapture;		
#else		
		return false;
#endif		
	}
	
#if UNITY_EDITOR	
		public MulticastDelegate cacheCompilerGenerated( MulticastDelegate dOther ) {
					
			if( dOther == null || dOther.Target == null ) {
				return null;
			}
			
			FieldInfo[] attrsA = dOther.Target.GetType().GetFields( BindingFlags.NonPublic | BindingFlags.Instance );	
			if( attrsA.Length > 0 ) {			
				return attrsA[0].GetValue( dOther.Target ) as MulticastDelegate;
			}
			return null;
		
		}	
#endif		
}
#endif