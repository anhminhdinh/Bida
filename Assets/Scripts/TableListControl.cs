using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TableListControl : MonoBehaviour
{
	public float MarginTop = 4;
	public float MarginLR = 4;
	public float MarginBottom = 4;
	public float ItemSpacing = 4;
	private float bottomPositionY;
	public Transform TableItem;
	public RectTransform CurrentRect;
	
	// Use this for initialization
	void Start()
	{
		init();
	}

	void init()
	{
		bottomPositionY = - MarginTop;
		CurrentRect = GetComponent<RectTransform>();
		RectTransform par = (RectTransform)this.transform.parent;
		CurrentRect.sizeDelta = new Vector2(CurrentRect.sizeDelta.x, par.rect.height);
		CurrentRect.anchoredPosition = new Vector2(CurrentRect.anchoredPosition.x, 0);
	}

	public void updateListItem(List<TableGame> tableList){
		foreach (Transform child in transform) {
			GameObject.Destroy(child.gameObject);
		}

		init();

		foreach (TableGame tableGame in tableList) {
			AddNewItem(tableGame);
		}
	}
	
	public void AddNewItem(TableGame tableGame)
	{
		RectTransform rect = (RectTransform)Instantiate(TableItem);
		rect.transform.parent = this.transform;

		rect.pivot = new Vector2(0.5f, 1);

		rect.offsetMax = new Vector2(0, rect.offsetMax.y);
		rect.offsetMin = new Vector2(MarginLR, rect.offsetMin.y);

		rect.anchoredPosition = new Vector2(0, bottomPositionY);
		
		bottomPositionY -= rect.rect.height + ItemSpacing;

		RectTransform textTransform = (RectTransform)rect.FindChild("Text");
		Text text = textTransform.GetComponent<Text>();
		text.text = tableGame.tostring();
		
		UpdateScrollHeight();
	}
	
	void UpdateScrollHeight()
	{
		if (CurrentRect.rect.height < -bottomPositionY) {
			CurrentRect.sizeDelta = new Vector2(CurrentRect.sizeDelta.x, -bottomPositionY + MarginBottom);
		}
	}
}
