using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections.Generic;

public class RoomListControl : MonoBehaviour
{
	public float MarginTop = 4;
	public float MarginLR = 4;
	public float MarginBottom = 4;
	public float ItemSpacing = 4;
	private float bottomPositionY;
	public Transform RoomItem;
	public RectTransform CurrentRect;
	
	// Use this for initialization
	void Start()
	{
		bottomPositionY = - MarginTop;
		CurrentRect = GetComponent<RectTransform>();
		RectTransform par = (RectTransform)this.transform.parent;
		CurrentRect.sizeDelta = new Vector2(CurrentRect.sizeDelta.x, par.rect.height);
		CurrentRect.anchoredPosition = new Vector2(CurrentRect.anchoredPosition.x, 0);
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
	
	public void AddNewItem(RoomGame roomGame)
	{
		RectTransform rect = (RectTransform)Instantiate(RoomItem);
		rect.transform.parent = this.transform;
		
		rect.pivot = new Vector2(0.5f, 1);

		rect.offsetMax = new Vector2(0, rect.offsetMax.y);
		rect.offsetMin = new Vector2(MarginLR, rect.offsetMin.y);
		
		rect.anchoredPosition = new Vector2(0, bottomPositionY);
		
		bottomPositionY -= rect.rect.height + ItemSpacing;

		Button button = rect.GetComponent<Button>();
		button.onClick.AddListener(() => { 
			Debug.Log(roomGame.name + " clicked"); 
			if (!roomGame.isFree)
				getListGamesOfRoom(roomGame);
			else if (GameApplication.user.ag > getMinAG())
				Debug.Log("Phòng Miễn phí chỉ dành cho các game thủ có tài sản dưới " + getMinAG()
				               + " AG! Bạn vui lòng sang các phòng chơi khác để chơi nhé ");
			else
				getListGamesOfRoom(roomGame);
		});
		
		RectTransform textTransform = (RectTransform)rect.FindChild("Text");
		Text text = textTransform.GetComponent<Text>();
		text.text = roomGame.name;
		
		UpdateScrollHeight();
	}
	
	void UpdateScrollHeight()
	{
		if (CurrentRect.rect.height < -bottomPositionY) {
			CurrentRect.sizeDelta = new Vector2(CurrentRect.sizeDelta.x, -bottomPositionY + MarginBottom);
		}
	}

	private void getListGamesOfRoom(RoomGame roomGame) {
		// TODO: can optimize lai viec find GameObject, co the dat vao CubeiaClient
		CubeiaClient cubeia = GameApplication.cubeia;
		GameObject tableListObject = GameObject.Find("TableList");
		TableListControl tableListControl = (TableListControl) tableListObject.GetComponent(typeof(TableListControl));
//		cubeia.tableList = new TableGame[] {};
//		if (User.getInstance().roomGame.length < 1)
//			return;
		cubeia.currentRoom = roomGame;
		// enter room default
		cubeia.tableList.Clear();
		tableListControl.updateListItem(cubeia.tableList);
		cubeia.sendSelectR(roomGame);
	}

	private int getMinAG() {
		return 100;
	}
}