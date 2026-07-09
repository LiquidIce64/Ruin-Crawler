using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinchStatusHUD : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    private const string ConnectedText = "Соединена";
    private const string DetachedText = "Отсоединена";

    private static readonly Color PanelColor = new(0.03f, 0.035f, 0.04f, 0.55f);
    private static readonly Color ConnectedSlotColor = new(0.06f, 0.16f, 0.1f, 0.82f);
    private static readonly Color DetachedSlotColor = new(0.04f, 0.045f, 0.05f, 0.68f);
    private static readonly Color ConnectedMarkerColor = new(0.22f, 0.92f, 0.47f, 1f);
    private static readonly Color DetachedMarkerColor = new(0.45f, 0.49f, 0.52f, 1f);
    private static readonly Color ConnectedTextColor = Color.white;
    private static readonly Color DetachedTextColor = new(0.68f, 0.72f, 0.76f, 1f);

    private CanvasGroup canvasGroup;
    private WinchSlotUi backSlot;
    private WinchSlotUi frontSlot;
    private bool? cachedBackAttached;
    private bool? cachedFrontAttached;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponentInParent<PlayerController>();

        BuildUi();
        SetVisible(false);
    }

    private void Update()
    {
        VehicleController vehicleController = playerController != null ? playerController.vehicleController : null;
        Winch backWinch = vehicleController != null ? vehicleController.backWinch : null;
        Winch frontWinch = vehicleController != null ? vehicleController.frontWinch : null;

        if (backWinch == null || frontWinch == null)
        {
            SetVisible(false);
            cachedBackAttached = null;
            cachedFrontAttached = null;
            return;
        }

        SetVisible(true);
        UpdateSlot(backSlot, backWinch.IsAttached, ref cachedBackAttached);
        UpdateSlot(frontSlot, frontWinch.IsAttached, ref cachedFrontAttached);
    }

    private void BuildUi()
    {
        GameObject panel = CreateUiObject("WinchStatusPanel", transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = new Vector2(0f, 58f);
        panelRect.sizeDelta = new Vector2(600f, 64f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = PanelColor;
        panelImage.raycastTarget = false;

        canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        backSlot = CreateSlot(panel.transform, "BackWinchStatus", "Q", "Задняя");
        frontSlot = CreateSlot(panel.transform, "FrontWinchStatus", "E", "Передняя");
    }

    private static WinchSlotUi CreateSlot(Transform parent, string name, string key, string title)
    {
        GameObject slot = CreateUiObject(name, parent);
        Image background = slot.AddComponent<Image>();
        background.raycastTarget = false;

        LayoutElement slotLayout = slot.AddComponent<LayoutElement>();
        slotLayout.preferredWidth = 286f;
        slotLayout.preferredHeight = 48f;

        HorizontalLayoutGroup layout = slot.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 7, 7);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TMP_Text keyText = CreateKeycap(slot.transform, key);
        TMP_Text titleText = CreateText(slot.transform, "Title", LocalizationManager.Translate(title), 17f, FontStyles.Bold, 76f);
        Image marker = CreateMarker(slot.transform);
        TMP_Text statusText = CreateText(slot.transform, "Status", LocalizationManager.Translate(DetachedText), 16f, FontStyles.Normal, 104f);

        return new WinchSlotUi
        {
            Background = background,
            Marker = marker,
            KeyText = keyText,
            TitleText = titleText,
            StatusText = statusText,
            RussianTitleText = title,
        };
    }

    private static TMP_Text CreateKeycap(Transform parent, string key)
    {
        GameObject keycap = CreateUiObject("Keycap", parent);
        Image keycapImage = keycap.AddComponent<Image>();
        keycapImage.color = new Color(0f, 0f, 0f, 0.45f);
        keycapImage.raycastTarget = false;

        LayoutElement layout = keycap.AddComponent<LayoutElement>();
        layout.preferredWidth = 34f;
        layout.preferredHeight = 34f;

        TMP_Text text = CreateText(keycap.transform, "Text", key, 20f, FontStyles.Bold, 34f);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        return text;
    }

    private static Image CreateMarker(Transform parent)
    {
        GameObject marker = CreateUiObject("Marker", parent);
        Image markerImage = marker.AddComponent<Image>();
        markerImage.raycastTarget = false;

        LayoutElement layout = marker.AddComponent<LayoutElement>();
        layout.preferredWidth = 12f;
        layout.preferredHeight = 12f;

        return markerImage;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, FontStyles style, float width)
    {
        GameObject textObject = CreateUiObject(name, parent);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;

        LayoutElement layout = textObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = 32f;

        return text;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.layer = parent.gameObject.layer;
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private void UpdateSlot(WinchSlotUi slot, bool attached, ref bool? cachedAttached)
    {
        if (cachedAttached.HasValue && cachedAttached.Value == attached)
            return;

        cachedAttached = attached;
        slot.Background.color = attached ? ConnectedSlotColor : DetachedSlotColor;
        slot.Marker.color = attached ? ConnectedMarkerColor : DetachedMarkerColor;
        slot.StatusText.text = LocalizationManager.Translate(attached ? ConnectedText : DetachedText);
        slot.TitleText.text = LocalizationManager.Translate(slot.RussianTitleText);
        slot.StatusText.color = attached ? ConnectedTextColor : DetachedTextColor;
        slot.TitleText.color = attached ? ConnectedTextColor : DetachedTextColor;
        slot.KeyText.color = attached ? ConnectedTextColor : DetachedTextColor;
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
    }

    private sealed class WinchSlotUi
    {
        public Image Background;
        public Image Marker;
        public TMP_Text KeyText;
        public TMP_Text TitleText;
        public TMP_Text StatusText;
        public string RussianTitleText;
    }
}
