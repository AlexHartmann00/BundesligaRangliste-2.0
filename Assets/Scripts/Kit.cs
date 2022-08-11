using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Kit : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    public LineRenderer linerenderer;
    public Player player;
    public Vector2 originalPosition;
    float minPitchWidth, maxPitchWidth;
    public float leftBoundary, rightBoundary;
    public float canvasWidth;
    public Canvas canvas;
    public LineupGUIManager manager;
    public GameObject bestPositionIndicator, highlightIndicator;
    bool showingIndicator = false;
    GameObject indicator;
    public Gradient positionGradient;
    public GameObject informationPanel;
    public Vector2 DragStartPosition;
    bool DragStartLinedUp = false;
    float timer = 0;
    public Sprite lockClosed, lockOpened, lineupForceExclamationMark;
    public Image lockToggleButtonImage;
    Vector3 dragStartPointerPosition;

    private void Awake()
    {
        canvasWidth = Screen.width;
        minPitchWidth = (canvasWidth / 2 + leftBoundary) / canvasWidth;
        maxPitchWidth = (canvasWidth / 2 + rightBoundary) / canvasWidth;
        Debug.Log("min: " + minPitchWidth + ", max: " + maxPitchWidth);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            highlightIndicator.SetActive(false);
        }
        Kit intersect = manager.IntersectingKit(GetComponent<RectTransform>().position, !player.away);
        if (intersect != null)
        {
            intersect.Highlight(1);
        }
    }

    public void OnBeginDrag(PointerEventData data)
    {
        dragStartPointerPosition = data.pointerEnter.transform.position;
        DragStartLinedUp = player.linedUp ? true : false;
        DragStartPosition = transform.position;
        transform.SetAsLastSibling();
        Debug.Log("Setting DragPosition to " + DragStartPosition);
    }

    public void OnDrag(PointerEventData data)
    {

        SetDraggedPosition(data);
        UpdateKit(data.pointerDrag.transform.position);
        ShowIndicator(data);
        ShowDistanceLine(data);
    }

    public void OnEndDrag(PointerEventData data)
    {
        Vector2 pointerPos = data.pointerDrag.transform.position;
        Kit intersect = manager.IntersectingKit(GetComponent<RectTransform>().position, !player.away);
        Debug.Log(intersect == null ? "Intersection Null" : "Intersection " + intersect.player.name + ", " + pointerPos + ", "+ intersect.transform.position);
        if(intersect != null)
        {
            bool intersect_linedup = intersect.player.linedUp ? true : false;
            intersect.player.linedUp = DragStartLinedUp;
            player.linedUp = intersect_linedup;
            Debug.Log("Swap Lineup Status: " + intersect.player.name + " " + intersect.player.linedUp + ", " + player.name + " " + player.linedUp);
            Vector2 intersectPosition = new Vector2(intersect.transform.position.x,intersect.transform.position.y);
            intersect.transform.position = DragStartPosition;
            transform.position = intersectPosition;
            intersect.UpdateKit(dragStartPointerPosition,  updateLinedup: true);
        }
        GameObject.Destroy(indicator);
        showingIndicator = false;
        linerenderer.Destroy();
        UpdateKit(data.pointerDrag.transform.position);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if(data.clickCount >= 2)
        {
            ShowInformationPanel();
        }
    }

    public void ShowInformationPanel()
    {
        transform.SetAsLastSibling();
        transform.parent.SetAsLastSibling();
        informationPanel.SetActive(true);
    }

    public void CloseInformationPanel()
    {
        informationPanel.SetActive(false);
        transform.SetAsFirstSibling();
    }

    void UpdateKit(Vector3 pointerPos, bool updateLinedup = true)
    {
        float width = Screen.width;
        float height = Screen.height;
        Vector3 point = pointerPos;
        float y = 1 - 2 * (Mathf.Abs(point.y / height - 0.5f));
        //y /= (float)PITCH_HEIGHT;
        float x = point.x / width;
        if(updateLinedup)player.linedUp = x > minPitchWidth && x < maxPitchWidth;
        if (player.linedUp)
        {
            float xx = 0.5f + (x - 0.5f) * (0.5f / (maxPitchWidth - 0.5f));
            //x /= (float)PITCH_WIDTH;
            Debug.Log(x + ", " + y + ", " + xx + " - DEBUG_KITDRAG");
            //y = y - 0.55f;
            float pas = RankingGlobal.Constants.PENALTY_AREA_START;
            float mof = RankingGlobal.Constants.MAX_OFFENSIVENESS;
            player.offensiveness = y < pas ? 0 : y;// y < pas ? 0 : (y - pas) * 1f / (1f - pas);
            Debug.Log(y + " --> " + player.offensiveness + " - DEBUG_KITDRAG");

            player.skewness = xx;
        }

        UpdateStarRating();
    }

    private void SetDraggedPosition(PointerEventData data)
    {
        RectTransform m_DraggingPlane = null;
        if (data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            m_DraggingPlane = data.pointerEnter.transform as RectTransform;

        var rt = GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane,data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
        }
    }

    public void UpdateStarRating()
    {
        GameObject rating = transform.Find("rating").gameObject;
        rating.GetComponent<RectTransform>().sizeDelta = new Vector2((player.GetTrueValue() - manager.min) / (manager.max - manager.min) * 59f, 19);
    }

    void ShowIndicator(PointerEventData data)
    {
        if(player.GetBestPosition() != Vector2.zero)
        {
            GameObject.Destroy(indicator);
            showingIndicator = true;
            Vector2 pos = player.GetBestPosition();
            pos.y = player.away ? 1f - 0.5f * pos.y : pos.y * 0.5f;
            pos.x = player.away ? 1f - pos.x : pos.x;
            float x = (pos.x + 0.25f / (maxPitchWidth - 0.5f) - 0.5f) * ((float)Screen.width * maxPitchWidth - 0.5f * (float)Screen.width) * 2f;
            Vector2 target = new Vector2(x, pos.y * (float)Screen.height);
            Debug.Log("Target vector : " + target);
            indicator = Instantiate(bestPositionIndicator, target, Quaternion.identity, transform);
            indicator.transform.SetAsFirstSibling();
        }
    }

    void ShowDistanceLine(PointerEventData data)//TODO: STILL NOT WORKING RIGHT
    {
        if(player.GetBestPosition() != Vector2.zero)
        {
            float diff = Mathf.Clamp01((player.GetPosition() - player.GetBestPosition()).magnitude);
            Color c = positionGradient.Evaluate(diff);
            linerenderer.Destroy();
            Vector2 pos = player.GetBestPosition();
            pos.y = player.away ? 1f - 0.5f * pos.y : pos.y * 0.5f;
            pos.x = player.away ? 1f - pos.x : pos.x;
            linerenderer.SetSize(10);
            float x = (pos.x + 0.25f / (maxPitchWidth - 0.5f) - 0.5f) * ((float)Screen.width * maxPitchWidth - 0.5f * (float)Screen.width) * 2f;

            x = (0.5f + (pos.x - 0.5f)/(0.5f/(maxPitchWidth - 0.5f)));
            Debug.Log("Kit position: " + RectTransformUtility.PixelAdjustPoint(GetComponent<RectTransform>().position,transform,canvas));
            Vector2 target = new Vector2((float)Screen.width * x, (float)Screen.height * pos.y);
            linerenderer.SetStartingPoint(target);//target);
            Vector2 p = RectTransformUtility.PixelAdjustPoint(GetComponent<RectTransform>().position,transform,canvas);
            float xScale = 1f - (float)Screen.width / 1920f;
            float yScale = 1f - (float)Screen.height / 1080f;
            Vector2 pAdj = p + new Vector2(xScale * (p.x - target.x), yScale * (p.y - target.y));
            linerenderer.SetEndPoint(pAdj);//new Vector2((1920f * p.x) / (float)Screen.width, (1080f * p.y) / (float)Screen.height));
            linerenderer.Draw(transform.parent);
            linerenderer.SetColor(c);

        }

    }

    public void Highlight(float seconds)
    {
        highlightIndicator.SetActive(true);
        timer = 2 * Time.deltaTime;
    }

    public void ToggleLineupLock()
    {
        player.lineupStatus = player.lineupStatus.Next();
        Sprite replacement = lockClosed;
        if(player.lineupStatus == LineupStatus.Available)
        {
            replacement = lockOpened;
        }
        else if(player.lineupStatus == LineupStatus.Locked)
        {
            replacement = lockClosed;
        }
        else if(player.lineupStatus == LineupStatus.Forced)
        {
            replacement = lineupForceExclamationMark;
        }
        lockToggleButtonImage.sprite = replacement;
    }
}
