using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class index : MonoBehaviour
{
    //单例
    public static index instance;

    public RectTransform _view;

    //行列
    public int tableRow = 5;

    public int tableColumn = 5;

    // Start is called before the first frame update
    public int _count = 6;

    //所有的Item
    public Item[,] allItems;

    //所有Item的坐标
    public Vector3[,] allPos;

    //偏移量
    public Vector2 offset = new Vector2(0, 0);

    //要消除的Item列表
    public List<Item> boomList;

    //相同Item列表
    public List<Item> sameItemsList;

    //正在操作
    public bool isOperation = false;

    public Text _text;

    //随机图案
    public Sprite[] randomSprites;

    void Start()
    {
        instance = this;
        allItems = new Item[tableRow, tableColumn];
        allPos = new Vector3[tableRow, tableColumn];

        sameItemsList = new List<Item>();
        boomList = new List<Item>();

        InitItem();
        InitGame();
        _text.text = "0";
    }

    public int[,] _data = new int[7, 7];

    private void Check_Score(int _score)
    {
        //_text.text =( int.Parse(_text.text) + _score  ).ToString();
        _text.text = _text.text + "~~" + _score;
    }


    private void InitItem()
    {
        for (int i = 0; i < tableRow; i++)
        {
            for (int j = 0; j < tableColumn; j++)
            {
                //随机图案编号
                int random = Random.Range(0, _count);
                //判断上下左右是否有两个连续的,有则更换一个数字


                //_data[i,j] = random;
                _data[i, j] = CheckItem(random, i, j);
            }
        }
    }

    /// <summary>
    /// 判断四周是否有连续两个
    /// </summary>
    private int CheckItem(int itemid, int _x, int _y)
    {
        List<int> _itemdata = new List<int>();

        if (_x > 1)
        {
            //左
            if (_data[_x - 1, _y] == itemid)
            {
                if (_data[_x - 2, _y] == itemid)
                {
                    _itemdata.Add(itemid);
                }
            }
        }

        if (_y > 1)
        {
            //上
            if (_data[_x, _y - 1] == itemid)
            {
                if (_data[_x, _y - 2] == itemid)
                {
                    _itemdata.Add(itemid);
                }
            }
        }

        if (_itemdata.Count > 0)
        {
            return RangeDate(_itemdata);
        }
        else
        {
            return itemid;
        }
    }


    private int RangeDate(List<int> _list)
    {
        int random = Random.Range(0, _count);
        if (_list.Contains(random))
        {
            Debug.LogError("重复了" + random);
            return RangeDate(_list);
            // return  666;
        }

        Debug.LogError("未重复" + random);
        return random;
    }


    /// <summary>
    /// 填充相同Item列表
    /// </summary>
    public void FillSameItemsList(Item current, bool _state = true)
    {
        //如果已存在，跳过
        if (sameItemsList.Contains(current))
            return;
        //添加到列表
        sameItemsList.Add(current);
        //上下左右的Item
        Item[] tempItemList = new Item[]
        {
            GetUpItem(current), GetDownItem(current),
            GetLeftItem(current), GetRightItem(current)
        };

        for (int i = 0; i < tempItemList.Length; i++)
        {
            //如果Item不合法，跳过
            if (tempItemList[i] == null)
                continue;


            if (current.currentSpr == tempItemList[i].currentSpr)
            {
                FillSameItemsList(tempItemList[i], _state);
            }

            if (_state)
            {
                FillBoomList(tempItemList[i]);
            }

            //            if (current._text.text.Equals(tempItemList[i]._text.text) )
            //            {
            //                FillSameItemsList(tempItemList[i]);
            //            }
        }
    }


    /// <summary>
    /// 填充待消除列表
    /// </summary>
    /// <param name="current">Current.</param>
    public bool FillBoomList(Item current, bool _state = true)
    {
        // Debug.LogError("填充待消除列表");
        //计数器
        int rowCount = 0;
        int columnCount = 0;
        //临时列表
        List<Item> rowTempList = new List<Item>();
        List<Item> columnTempList = new List<Item>();
        ///横向纵向检测
        foreach (var item in sameItemsList)
        {
            //如果在同一行
            if (item.itemRow == current.itemRow)
            {
                //判断该点与Curren中间有无间隙
                bool rowCanBoom = CheckItemsInterval(true, current, item);
                if (rowCanBoom)
                {
                    //计数
                    rowCount++;
                    //添加到行临时列表
                    rowTempList.Add(item);
                }
            }

            //如果在同一列
            if (item.itemColumn == current.itemColumn)
            {
                //判断该点与Curren中间有无间隙
                bool columnCanBoom = CheckItemsInterval(false, current, item);
                if (columnCanBoom)
                {
                    //计数
                    columnCount++;
                    //添加到列临时列表
                    columnTempList.Add(item);
                }
            }
        }

        //横向消除
        bool horizontalBoom = false;
        //如果横向三个以上
        if (rowCount > 2)
        {
            Check_Score(rowCount);
            Debug.LogError("如果横向三个以上" + rowCount);
            //将临时列表中的Item全部放入BoomList
            boomList.AddRange(rowTempList);
            //横向消除
            horizontalBoom = true;
        }

        //如果纵向三个以上
        if (columnCount > 2)
        {
            Check_Score(columnCount);
            Debug.LogError("如果纵向三个以上" + columnCount);
            if (horizontalBoom)
            {
                //剔除自己
                boomList.Remove(current);
            }

            //将临时列表中的Item全部放入BoomList
            boomList.AddRange(columnTempList);
        }


        //如果没有消除对象，返回
        if (boomList.Count == 0)
            return false;
        //创建临时的BoomList
        //创建临时的BoomList
        if (_state)
        {
            List<Item> tempBoomList = new List<Item>();
            tempBoomList.Clear();
            //转移到临时列表
            tempBoomList.AddRange(boomList);
            //开启处理BoomList的协程
            StartCoroutine(ManipulateBoomList(tempBoomList));
        }

        return true;
    }

    /// <summary>
    /// 移动并删除
    /// </summary>
    private void Move_Bool()
    {
        List<Item> tempBoomList = new List<Item>();
        tempBoomList.Clear();
        //转移到临时列表
        tempBoomList.AddRange(boomList);
        //开启处理BoomList的协程
        StartCoroutine(ManipulateBoomList(tempBoomList));
    }


    /// <summary>
    /// 处理BoomList
    /// </summary>
    /// <returns>The boom list.</returns>
    IEnumerator ManipulateBoomList(List<Item> tempBoomList)
    {
        Debug.LogError("处理删除");
        foreach (var item in tempBoomList)
        {
            item.hasCheck = true;
            //  item.GetComponent<Image>().color = randomColor * 2;
            //离开动画
            // item.GetComponent<AnimatedButton>().Exit();
            //Boom声音
            //  AudioManager.instance.PlayMagicalAudio();
            //将被消除的Item在全局列表中移除
            allItems[item.itemRow, item.itemColumn] = null;
            //item. transform.DOShakeScale(0.5f, Vector3.one * 1.1f);
            //  item.GetComponent<RectTransform>().DOPunchScale(new Vector3(1.1f,1.1f), 0.3f);
            item.transform.GetComponent<Image>().rectTransform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.1f)
                .OnComplete((() => { item.transform.GetComponent<Image>().rectTransform.DOScale(Vector3.one, 0.1f); }));
//            item.GetComponent<RectTransform>().DOPunchScale(new Vector3(1.1f,1.1f), 0.3f);
//            item.GetComponent<RectTransform>().DOLocalRotate(new Vector3(1.1f, 1.1f), 0.3f);
        }
        //检测Item是否已经开发播放离开动画
        //        while (!tempBoomList[0].GetComponent<AnimatedButton>().CheckPlayExit())
        //        {
        //            yield return 0;
        //        }


        //延迟0.2秒
        yield return new WaitForSeconds(0.3f);
        foreach (var item in tempBoomList)
        {
            //回收Item
            ObjectPool.instance.SetGameObject(item.gameObject);
        }

        //开启下落
        yield return StartCoroutine(ItemsDrop());
        //延迟0.38秒
        yield return new WaitForSeconds(0.38f);
    }


    /// <summary>
    /// Items下落
    /// </summary>
    /// <returns>The drop.</returns>
    IEnumerator ItemsDrop()
    {
        Debug.LogError("开启item下落");
        isOperation = true;
        //逐列检测
        for (int i = 0; i < tableColumn; i++)
        {
            //计数器
            int count = 0;
            //下落队列
            Queue<Item> dropQueue = new Queue<Item>();
            //逐行检测
            for (int j = 0; j < tableRow; j++)
            {
                if (allItems[j, i] != null)
                {
                    //计数
                    count++;
                    //放入队列
                    dropQueue.Enqueue(allItems[j, i]);
                }
            }

            //下落
            for (int k = 0; k < count; k++)
            {
                //获取要下落的Item
                Item current = dropQueue.Dequeue();
                //修改全局数组(原位置情况)
                allItems[current.itemRow, current.itemColumn] = null;
                //修改Item的行数
                current.itemRow = k;
                //修改全局数组(填充新位置)
                allItems[current.itemRow, current.itemColumn] = current;
                //下落
                current.GetComponent<ItemOperation>().CurrentItemDrop(allPos[current.itemRow, current.itemColumn]);
            }
        }

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(CreateNewItem());
        yield return new WaitForSeconds(0.3f);
        AllBoom();
    }


    void AllBoom()
    {
        Debug.LogError("全局检查是否有消除");
        //有消除
        bool hasBoom = false;
        foreach (var item in allItems)
        {
            //指定位置的Item存在，且没有被检测过
            if (item && !item.hasCheck)
            {
                //检测周围的消除
                if (item.CheckAroundBoom())
                {
                    Debug.LogError("检查到可消除,不再全局检测");
                    break;
                }


                if (boomList.Count > 0)
                {
                    hasBoom = true;
                    isOperation = true;
                }
            }
        }

        if (!hasBoom)
        {
            //操作结束
            isOperation = false;
        }
    }

    #region 是否有可消除

    

    #endregion
    public void Check_item()
    {
        _Check_item(allItems);
    }


    private void _Check_item(Item[,] _TemallItems)
    {
        //  Item[,] _TemallItems = allItems;

        foreach (var item in _TemallItems)
        {
            Item[] _tempItemList = new Item[]
            {
                GetUpItem(item), GetDownItem(item),
                GetLeftItem(item), GetRightItem(item)
            };

            for (int i = 0; i < _tempItemList.Length; i++)
            {
                if (_tempItemList[i] == null)
                    continue;

                sameItemsList.Clear();
                boomList.Clear();
                Item May_item = _tempItemList[i];

                Sprite _sprite = May_item.currentSpr;
                May_item.currentSpr = item.currentSpr;
                item.currentSpr = _sprite;

                FillSameItemsList(_tempItemList[i], false);
                if (FillBoomList(_tempItemList[i], false))
                {
                    Debug.LogError("有可消除的");
                }
            }
        }


        Debug.LogError("没有可消除的");
    }


    #region 自动消除

    public void Automatic()
    {
        StartCoroutine(AutomaticBoom());
    }


    private IEnumerator AutomaticBoom()
    {
        Debug.LogError("开始");
        //有消除
        bool hasBoom = false;
        Item[,] _TemallItems = allItems;

        foreach (var item in _TemallItems)
        {
            Debug.LogError("开始检测" + item.itemRow + "行" + item.itemColumn + "列");


            Item[] _tempItemList = new Item[]
            {
                GetUpItem(item), GetDownItem(item),
                GetLeftItem(item), GetRightItem(item)
            };

            for (int i = 0; i < _tempItemList.Length; i++)
            {
                if (_tempItemList[i] == null)
                    continue;

                sameItemsList.Clear();
                boomList.Clear();
                Item May_item = _tempItemList[i];

                Sprite _sprite = May_item.currentSpr;
                May_item.currentSpr = item.currentSpr;
                item.currentSpr = _sprite;

                FillSameItemsList(_tempItemList[i], false);
                if (FillBoomList(_tempItemList[i], false))
                {
                    Debug.LogWarning("检测到可消除");
                    Debug.LogError(_tempItemList[i].itemRow);
                    Debug.LogError(_tempItemList[i].itemColumn);
                    Vector2 _prontV = new Vector2(-(_tempItemList[i].itemColumn - item.itemColumn),
                        -(_tempItemList[i].itemRow - item.itemRow));
                    Debug.LogError(_prontV);
                    item._text.text = "中了";

                    item.currentSpr = May_item.currentSpr;
                    May_item.currentSpr = _sprite;


                    item.transform.DOPunchScale(Vector3.one * 1.1f, 0.3f);
                    May_item.transform.DOPunchScale(Vector3.one * 1.1f, 0.3f);


                    //                    May_item.GetComponent<ItemOperation>().ItemMove(item.itemRow, item.itemColumn, item.transform.position);
                    //                    item.GetComponent<ItemOperation>().ItemMove(May_item.itemRow, May_item.itemColumn, May_item.transform.position);
                    yield return new WaitForSeconds(0.5f);


                    May_item.GetComponent<ItemOperation>().MovieItem(_prontV);
                    yield break;
                }
                else
                {
                    //  Debug.LogWarning("未可消除 更换");
                    item.currentSpr = May_item.currentSpr;
                    May_item.currentSpr = _sprite;
                }
            }
        }

        Debug.LogError("1111111111111");
        if (!hasBoom)
        {
            Debug.LogError("操作结束");
            //操作结束
            isOperation = false;
        }
    }

    #endregion


    /// <summary>
    /// 生成新的Item
    /// </summary>
    /// <returns>The new item.</returns>
    public IEnumerator CreateNewItem()
    {
        Debug.LogError("生成新的Item");
        isOperation = true;
        for (int i = 0; i < tableColumn; i++)
        {
            int count = 0;
            Queue<GameObject> newItemQueue = new Queue<GameObject>();
            for (int j = 0; j < tableRow; j++)
            {
                if (allItems[j, i] == null)
                {
                    //生成一个Item
                    //GameObject current = (GameObject)Instantiate(Resources. Load<GameObject>("Prefabs/Item"));
                    GameObject current = ObjectPool.instance.GetGameObject("Item", transform, false);


                    // GameObject current = ObjectPool.instance.GetGameObject("Item", transform);

                    //						ObjectPool.instance.GetGameObject (Util.Item, transform);
                    current.transform.SetParent(transform);
                    current.transform.position = allPos[tableRow - 1, i];

                    newItemQueue.Enqueue(current);
                    count++;
                }
            }

            for (int k = 0; k < count; k++)
            {
                //获取Item组件
                Item currentItem = newItemQueue.Dequeue().GetComponent<Item>();

                //随机数
                int random = Random.Range(0, _count);
                currentItem._text.text = random.ToString();
                Debug.LogError("生成了" + count + "个" + random);
                //修改脚本中的图片
                currentItem.currentSpr = randomSprites[random];
                //修改真实图片
                currentItem.currentImg.sprite = randomSprites[random];
                //获取要移动的行数
                int r = tableRow - count + k;
                //移动
                currentItem.GetComponent<ItemOperation>().ItemMove(r, i, allPos[r, i]);
            }
        }

        isOperation = false;

        yield break;
    }


    /// <summary>
    /// 检测两个Item之间是否有间隙（图案不一致）
    /// </summary>
    /// <param name="isHorizontal">是否是横向.</param>
    /// <param name="begin">检测起点.</param>
    /// <param name="end">检测终点.</param>
    private bool CheckItemsInterval(bool isHorizontal, Item begin, Item end)
    {
        //获取图案
        Sprite spr = begin.currentSpr;
        //如果是横向
        if (isHorizontal)
        {
            //起点终点列号
            int beginIndex = begin.itemColumn;
            int endIndex = end.itemColumn;
            //如果起点在右，交换起点终点列号
            if (beginIndex > endIndex)
            {
                beginIndex = end.itemColumn;
                endIndex = begin.itemColumn;
            }

            //遍历中间的Item
            for (int i = beginIndex + 1; i < endIndex; i++)
            {
                //异常处理（中间未生成，标识为不合法）
                if (allItems[begin.itemRow, i] == null)
                    return false;
                //如果中间有间隙（有图案不一致的）
                if (allItems[begin.itemRow, i].currentSpr != spr)
                {
                    return false;
                }
            }

            return true;
        }
        else
        {
            //起点终点行号
            int beginIndex = begin.itemRow;
            int endIndex = end.itemRow;
            //如果起点在上，交换起点终点列号
            if (beginIndex > endIndex)
            {
                beginIndex = end.itemRow;
                endIndex = begin.itemRow;
            }

            //遍历中间的Item
            for (int i = beginIndex + 1; i < endIndex; i++)
            {
                //如果中间有间隙（有图案不一致的）
                if (allItems[i, begin.itemColumn].currentSpr != spr)
                {
                    return false;
                }
            }

            return true;
        }
    }


    // Update is called once per frame
    void Update()
    {
    }

    private float GetItemSize()
    {
        //return _view.rect.height;
        return 80;
    }

    //ITEM的边长
    private float itemSize = 0;

    private void InitGame()
    {
        //获取Item边长
        itemSize = GetItemSize();
        //生成ITEM
        for (int i = 0; i < tableRow; i++)
        {
            for (int j = 0; j < tableColumn; j++)
            {
                //生成
                GameObject currentItem = ObjectPool.instance.GetGameObject("Item", transform);
                //设置坐标
                currentItem.transform.localPosition =
                    new Vector3(j * itemSize, i * itemSize, 0) + new Vector3(offset.x, offset.y, 0);
                //随机图案编号
                // int random = Random.Range(0, count);
                //获取Item组件
                Item current = currentItem.GetComponent<Item>();
                //设置行列
                current.itemRow = i;
                current.itemColumn = j;
                current._text.text = _data[i, j].ToString();
                int random = _data[i, j];
                //设置图案
                current.currentSpr = randomSprites[random];
                //设置图片
                current.currentImg.sprite = randomSprites[random];
                //保存到数组
                allItems[i, j] = current;
                //记录世界坐标
                allPos[i, j] = currentItem.transform.position;
            }
        }
    }

    /// <summary>
    /// 检测行列是否合法
    /// </summary>
    /// <returns><c>true</c>, if RC legal was checked, <c>false</c> otherwise.</returns>
    /// <param name="itemRow">Item row.</param>
    /// <param name="itemColumn">Item column.</param>
    public bool CheckRCLegal(int itemRow, int itemColumn)
    {
        if (itemRow >= 0 && itemRow < tableRow && itemColumn >= 0 && itemColumn < tableColumn)
            return true;
        return false;
    }

    /// <summary>
    /// 获取上方Item
    /// </summary>
    /// <returns>The up item.</returns>
    /// <param name="current">Current.</param>
    private Item GetUpItem(Item current)
    {
        int row = current.itemRow + 1;
        int column = current.itemColumn;
        if (!CheckRCLegal(row, column))
            return null;
        return allItems[row, column];
    }

    /// <summary>
    /// 获取下方Item
    /// </summary>
    /// <returns>The down item.</returns>
    /// <param name="current">Current.</param>
    private Item GetDownItem(Item current)
    {
        int row = current.itemRow - 1;
        int column = current.itemColumn;
        if (!CheckRCLegal(row, column))
            return null;
        return allItems[row, column];
    }

    /// <summary>
    /// 获取左方Item
    /// </summary>
    /// <returns>The left item.</returns>
    /// <param name="current">Current.</param>
    private Item GetLeftItem(Item current)
    {
        int row = current.itemRow;
        int column = current.itemColumn - 1;
        if (!CheckRCLegal(row, column))
            return null;
        return allItems[row, column];
    }

    /// <summary>
    /// 获取右方Item
    /// </summary>
    /// <returns>The right item.</returns>
    /// <param name="current">Current.</param>
    private Item GetRightItem(Item current)
    {
        int row = current.itemRow;
        int column = current.itemColumn + 1;
        if (!CheckRCLegal(row, column))
            return null;
        return allItems[row, column];
    }


    #region 洗牌

    private void shufflise()
    {
        //生成一个新的组
        InitItem();
       //验证这个组是否可消



    }
    

    #endregion





    
}