﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class Node : ScriptableObject {

	[ContextMenuItem("Reset protected values", "Awake")]
	public string nodeUITitle = "Node";
	public Rect nodeUIRect;
	private bool isDragged = false;
	protected bool isSelected;
	[NonSerialized] public NodeUIGraph graph;
	public List<NodeConnector> inputConnections;
	public List<NodeConnector> outputs;

	private float lineHeight;
	protected GUIStyle normalStyle;
	protected GUIStyle selectedStyle;
	protected GUIStyle inputConnectorStyle;
	protected GUIStyle inputConnectorConnectedStyle;
	protected GUIStyle outputConnectorStyle;
	protected GUIStyle outputConnectorConnectedStyle;

	//testing
	public string prop;
	public int prop2;
	public GameObject proprtyNumberThree;

	public Node() {}

	private void Awake() {

		// setup style
		normalStyle = new GUIStyle();
		normalStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
		normalStyle.border = new RectOffset(12, 12, 12, 12);

		selectedStyle = new GUIStyle();
		selectedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 pn.png") as Texture2D;
		selectedStyle.border = new RectOffset(15, 15, 15, 15);
		inputConnectorStyle = new GUIStyle();
		inputConnectorConnectedStyle = new GUIStyle();
		outputConnectorStyle = new GUIStyle();
		outputConnectorConnectedStyle = new GUIStyle();
		lineHeight = EditorGUIUtility.singleLineHeight + 2;
		if (inputConnections == null) {
			inputConnections = new List<NodeConnector>();
		}
		if (outputs == null) {
			outputs = new List<NodeConnector>();
		}
	}

	public void NodeDraw(SerializedObject me) {
		me.Update();
		// if (node.GetType().IsSubclassOf(typeof(PropertyNode)) || node.GetType()==typeof(PropertyNode))
		Rect rect = me.FindProperty("nodeUIRect").rectValue;
		GUI.Box(rect, me.FindProperty("nodeUITitle").stringValue); //, isSelected ? selectedStyle : normalStyle);
		SerializedProperty sp = me.GetIterator();
		sp.Next(true);
		sp.NextVisible(true); // ignore Script
		sp.NextVisible(false); // ignore title
		sp.NextVisible(false); // ignore rect
		sp.NextVisible(false); // ignore input connections
		sp.NextVisible(false); // ignore outputs
		Rect nextPropRect = new Rect(rect.x + 10, rect.y + 5, rect.width / 5, lineHeight);
		float boxHeight = 10;
		Rect boxRect = new Rect(nextPropRect.x - 10, nextPropRect.y + boxHeight / 2, boxHeight, boxHeight);
		int inputCounter = 0;
		while (sp.NextVisible(false)) {
			GUI.Box(boxRect, GUIContent.none); //, inputConnectorStyle);
			EditorGUI.LabelField(nextPropRect, sp.displayName);
			if (inputConnections.Count <= inputCounter || inputConnections[inputCounter] == null) {
				float width = EditorGUIUtility.fieldWidth;
				Rect emptyPropertyBox = new Rect(nextPropRect.x - width - 10, nextPropRect.y, width, nextPropRect.height);
				GUI.Box(emptyPropertyBox, GUIContent.none);
				EditorGUI.PropertyField(emptyPropertyBox, sp, GUIContent.none);
			}
			nextPropRect.y += lineHeight;
			boxRect.y += lineHeight;
			inputCounter++;
		}
		rect.height = nextPropRect.y - rect.y;
		me.FindProperty("nodeUIRect").rectValue = rect;
		me.ApplyModifiedProperties();
		// me.Update();
		graph.Save();
	}
	public void ProcessConnectorEvents(Event e, Rect box) {
		switch (e.type) {
			case EventType.MouseDown:
				if (box.Contains(e.mousePosition)) {
					if (e.button == 0) {
						e.Use();
						// try to start new connection
						// graph.
					}
				} else {
					if (e.button == 0) {
						GUI.changed = true;
					}
				}
				break;
		}
	}

	public void ProcessEvents(Event e) {
		for (int i = 0; i < inputConnections.Count; i++) {
			if (inputConnections[i] != null) {
				Rect box = new Rect();
				ProcessConnectorEvents(e, box);
			}
		}
		for (int i = 0; i < outputs.Count; i++) {
			Rect box = new Rect();
			ProcessConnectorEvents(e, box);
		}
		switch (e.type) {
			case EventType.MouseDown:
				if (nodeUIRect.Contains(e.mousePosition)) {
					if (e.button == 0) {
						isDragged = true;
						isSelected = true;
						Selection.activeObject = this;
						GUI.changed = true;
						graph.BringToFront(this);
						e.Use();
					} else if (e.button == 1) {
						isSelected = true;
						GUI.changed = true;
						GenericMenu contextMenu = new GenericMenu();
						HandleContextMenu(contextMenu, e);
						e.Use();
					}
				} else {
					if (e.button == 0) {
						isSelected = false;
						GUI.changed = true;
					}
				}
				break;
			case EventType.MouseUp:
				isDragged = false;
				break;
			case EventType.MouseDrag:
				if (e.button == 0 && isDragged) {
					Drag(e.delta);
					e.Use();
					GUI.changed = true;
				}
				break;
		}
	}
	private void Drag(Vector2 delta) {
		nodeUIRect.position += delta;
	}
	protected void HandleContextMenu(GenericMenu contextMenu, Event e) {
		contextMenu.AddItem(new GUIContent("Remove Node"), false, () => { graph.RemoveNode(this); });

		contextMenu.ShowAsContext();
	}

	public void AddOutput<T>(string name, T defaultValue = default(T)) {
		outputs.Add(new NodeConnector<T>(name, defaultValue));
	}

	public void SetOutput<T>(string name, T value) {
		NodeConnector nodeOutput = outputs.Find((no) => { return no.propertyName == name; });
		if (nodeOutput != null) {
			((NodeConnector<T>) nodeOutput).value = value;
		}
	}

	public void GetConnectionValues() {

	}

	public virtual void Start() {}
	public virtual void OnGUI() {}
}

[Serializable]
public class NodeConnector : ScriptableObject {

	public string propertyName;
	public NodeConnector connectedInput;
	public NodeConnector(string name) {
		this.propertyName = name;
	}
	public T GetValue<T>() {
		return ((NodeConnector<T>) this).value;
	}
	public virtual void SetValue<T>(T t) {
		((NodeConnector<T>) this).value = t;
	}
}
public class NodeConnector<T> : NodeConnector {
	public T value;
	public T defaultValue;
	public NodeConnector(string name, T defaultValue) : base(name) {
		this.defaultValue = defaultValue;
		this.value = this.defaultValue;
	}
}