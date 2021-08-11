using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour {

	public int itemRow;//行
	public int itemColumn;//列
	//当前图案
	public Sprite currentSpr;
	//图案
	public Image currentImg;

	private index controller;

    public Text _text;
	//被检测
	public bool hasCheck = false;

	void Awake()
	{
		currentImg = transform.GetChild (0).GetComponent<Image> ();
        _text = transform.GetChild (1).GetComponent<Text> ();
	}

	void OnEnable()
	{
		controller = index.instance;
	}

	/// <summary>
	/// 点击事件
	/// </summary>
	public bool CheckAroundBoom(bool _state = true)
	{
		controller.sameItemsList.Clear ();
		controller.boomList.Clear ();
		//controller.randomColor = Color.white;
			//new Color (Random.Range (0.1f, 1f), Random.Range (0.1f, 1f), Random.Range (0.1f, 1f), 1);
		controller.FillSameItemsList (this, _state);
      //  Debug.LogError("点击事件  "+ _text.text);
//        foreach (Item item in controller.sameItemsList)
//        {
//            Debug.LogError(item.itemColumn);
//            Debug.LogError(item.itemRow);
//            Debug.LogError("=================");
//        }
		return controller.FillBoomList (this, _state);
	}
}