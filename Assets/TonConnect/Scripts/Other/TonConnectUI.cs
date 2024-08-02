using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TonSdk.Connect;
using TonSdk.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Message = TonSdk.Connect.Message;

public class TonConnectUI : MonoBehaviour
{
    [Tooltip("Toggle if you want to use presaved wallet icons. (recommended)")]
    public bool UseSavedWalletIcons = true;
    [Tooltip("Wallet icons. Works only if UseSavedWalletIcons is enabled.")]
    public List<Sprite> WalletIcons = new();
    private List<string> WalletsIconsList = new() { "tonkeeper", "tonhub", "openmask", "dewallet", "mytonwallet", "tonflow", "tonwallet", "xtonwallet", "telegram-wallet" };


    [Header("Old UI References")]
    [SerializeField] private GameObject _walletConnectionPanel;
    [SerializeField] private GameObject _walletConnectButton;
    [SerializeField] private GameObject _walletConnectedButton;

    [SerializeField] private RectTransform _walletItemParents;
    [SerializeField] private Image _walletItemPrefab;
    [SerializeField] private GameObject _walletSelectionPanel;
    [SerializeField] private GameObject _walletConnectQRPanel;
    [SerializeField] private RawImage _walletQRCode;


private string _currentWallet = "";

    [Header("References")]
    [SerializeField] private TonConnectHandler tonConnectHandler;

    private void Awake()
    {
        TonConnectHandler.OnProviderStatusChanged += OnProviderStatusChange;
        TonConnectHandler.OnProviderStatusChangedError += OnProviderStatusChangeError;
        DisableSendTXModal();
        DisableWalletInfoButton();
        EnableConnectWalletButton();

    }

    //callback when player successfully connect his account
    private void OnProviderStatusChange(Wallet wallet)
    {
        if (tonConnectHandler.tonConnect.IsConnected)
        {
            Debug.Log("Wallet connected. Address: " + wallet.Account.Address + ". Platform: " + wallet.Device.Platform + "," + wallet.Device.AppName + "," + wallet.Device.AppVersion);
            CloseConnectModal();
            DisableConnectWalletButton();
            EnableWalletInfoButton(ProcessWalletAddress(wallet.Account.Address.ToString(AddressType.Base64)));
        }
        else
        {
            EnableConnectWalletButton();
            DisableWalletInfoButton();
        }
    }

    private void OnProviderStatusChangeError(string message)
    {

    }

    #region Utilities

    private string ProcessWalletAddress(string address)
    {
        if (address.Length < 8) return address;

        string firstFourChars = address[..4];
        string lastFourChars = address[^4..];

        return firstFourChars + "..." + lastFourChars;
    }

    #endregion

    #region Button Click Events
    private async void OpenWalletQRContent(WalletConfig config)
    {



        string connectUrl = await tonConnectHandler.tonConnect.Connect(config);
        Texture2D qrCodeTexture = QRGenerator.EncodeString(connectUrl.ToString());

        // old ui style
        _walletConnectQRPanel.SetActive(true);
        _walletSelectionPanel.SetActive(false);
        UnityEngine.UI.Button openURLButon = _walletConnectQRPanel.GetComponentInChildren<UnityEngine.UI.Button>();
        openURLButon.onClick.AddListener(() => OpenWalletUrl(connectUrl));
        openURLButon.GetComponentInChildren<TextMeshProUGUI>().text = $"Open {config.Name}";

        _walletQRCode.texture = qrCodeTexture;
    }

    private async void OpenWebWallet(WalletConfig config)
    {
        await tonConnectHandler.tonConnect.Connect(config);
    }

    private void OpenWalletUrl(string url)
    {
        string escapedUrl = Uri.EscapeUriString(url);
        Application.OpenURL(escapedUrl);
    }


    private void BackToMainContent()
    {
        tonConnectHandler.tonConnect.PauseConnection();

    }

    public void GoBackFromQRCode()
    {
        _walletConnectQRPanel.SetActive(false);
        _walletSelectionPanel.SetActive(true);

    }


    public void CloseSelectPanel()
    {
        _walletConnectionPanel.SetActive(false);
    }
    public void CloseQRPanel()
    {
        _walletConnectionPanel.SetActive(false);
        _walletConnectQRPanel.SetActive(false);
        _walletSelectionPanel.SetActive(true);
    }

    public void ConnectWalletClick()
    {
        ShowConnectModal();
    }

    public void DisconnectWalletClick()
    {
        EnableConnectWalletButton();
        DisableWalletInfoButton();
        tonConnectHandler.RestoreConnectionOnAwake = false;
        tonConnectHandler.tonConnect.Disconnect();
    }

    public void WalletInfoClick()
    {
        ShowConnectModal();
    }

    private void ConnectWalletButtonClick()
    {

        ShowConnectModal();

    }

    private async void DisconnectWalletButtonClick()
    {
        EnableConnectWalletButton();
        DisableWalletInfoButton();
        tonConnectHandler.RestoreConnectionOnAwake = false;
        await tonConnectHandler.tonConnect.Disconnect();
    }

    private void WalletInfoButtonClick()
    {
        ShowSendTXModal();
    }

    private void CloseTXModalButtonClick()
    {
        DisableSendTXModal();
    }

    private async void SendTXModalSendButtonClick()
    {
        string receiverAddress = "";//document.rootVisualElement.Q<TextField>("SendTXModal_Address").value;
        double sendValue = 0;//document.rootVisualElement.Q<DoubleField>("SendTXModal_Value").value;
        if (string.IsNullOrEmpty(receiverAddress) || sendValue <= 0) return;

        Address receiver = new(receiverAddress);
        Coins amount = new(sendValue);
        Message[] sendTons =
        {
            new Message(receiver, amount),
            //new Message(receiver, amount),
            //new Message(receiver, amount),
            //new Message(receiver, amount),
        };

        long validUntil = DateTimeOffset.Now.ToUnixTimeSeconds() + 600;

        SendTransactionRequest transactionRequest = new SendTransactionRequest(sendTons, validUntil);
        await tonConnectHandler.tonConnect.SendTransaction(transactionRequest);
    }

    #endregion

    #region Tasks
    private void LoadWalletsCallback(List<WalletConfig> wallets)
    {
        // Here you can do something with the wallets list
        // for example: add them to the connect modal window
        // Warning! Use coroutines to load data from the web
        StartCoroutine(LoadWalletsIntoModal(wallets));
    }

    private IEnumerator LoadWalletsIntoModal(List<WalletConfig> wallets)
    {

        DeleteAllChildObjects(_walletItemParents);


        // load http bridge wallets
        foreach (var t in wallets)
        {
            if (t.BridgeUrl == null) continue;

            Image walletIcon = Instantiate(_walletItemPrefab, _walletItemParents);

            if (UseSavedWalletIcons && WalletsIconsList.Contains(t.AppName))
            {
                walletIcon.sprite = WalletIcons[WalletsIconsList.IndexOf(t.AppName)];
            }
            else
            {
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(t.Image))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                        Debug.LogError("Error while loading wallet image: " + request.error);
                    else
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(request);
                        if (texture != null) walletIcon.sprite = ToSprite(texture);

                    }
                }
            }
            walletIcon.GetComponentInChildren<TextMeshProUGUI>().text = t.Name;
            walletIcon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OpenWalletQRContent(t));

            //old

        }

        // load js bridge wallets
        if (tonConnectHandler.UseWebWallets)
        {
            for (int i = 0; i < wallets.Count; i++)
            {
                if (wallets[i].JsBridgeKey == null || !InjectedProvider.IsWalletInjected(wallets[i].JsBridgeKey)) continue;
                Image walletIcon = Instantiate(_walletItemPrefab, _walletItemParents);

                if (UseSavedWalletIcons && WalletsIconsList.Contains(wallets[i].AppName))
                {
                    walletIcon.sprite = WalletIcons[WalletsIconsList.IndexOf(wallets[i].AppName)];

                }
                else
                {
                    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(wallets[i].Image))
                    {
                        yield return request.SendWebRequest();

                        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                            Debug.LogError("Error while loading wallet image: " + request.error);
                        else
                        {
                            Texture2D texture = DownloadHandlerTexture.GetContent(request);
                            if (texture != null) walletIcon.sprite = ToSprite(texture);

                        }
                    }
                }

                int index = i;

                walletIcon.GetComponentInChildren<TextMeshProUGUI>().text = wallets[index].Name;
                walletIcon.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OpenWebWallet(wallets[index]));

                //old
            }
        }

    }
    #endregion

    #region UI Methods
    

    public void ShowConnectModal()
    {
       // ClipboardManager.CopyToClipboard("Successfully copied...");

        Debug.Log("Click on Connect Button");
        Debug.Log("Show Connect Modal 1");
        if (tonConnectHandler.tonConnect.IsConnected)
        {
            Debug.Log("Show Connect Modal 2");
            Debug.LogWarning("Wallet already connected. The connection window has not been opened. Before proceeding, please disconnect from your wallet.");
            return;
        }
        Debug.Log("Show Connect Modal 3");
        tonConnectHandler.CreateTonConnectInstance();


        Debug.Log("Show connect modal");
        _walletConnectionPanel.SetActive(true);
        _walletSelectionPanel.SetActive(true);

        Debug.Log("Show Connect Modal 4");

        Debug.Log("Show Connect Modal 5");
        StartCoroutine(tonConnectHandler.LoadWallets("https://raw.githubusercontent.com/ton-blockchain/wallets-list/main/wallets-v2.json", LoadWalletsCallback));
    }


    private void CloseConnectModal()
    {
        if (!tonConnectHandler.tonConnect.IsConnected) tonConnectHandler.tonConnect.PauseConnection();
        _walletConnectionPanel.SetActive(false);
    }

    private void EnableConnectWalletButton()
    {
        //old ui style
        _walletConnectButton.SetActive(true);
        _walletConnectedButton.SetActive(false);

        // enable connect button
    }

    

    public void CopyAdressToClipboard()
    {
        ClipboardManager.CopyToClipboard(_currentWallet);
    }

    private void EnableWalletInfoButton(string wallet)
    {

        //old ui style
        _walletConnectButton.SetActive(false);
        _walletConnectedButton.SetActive(true);
        _currentWallet = wallet;
        _walletConnectedButton.GetComponentInChildren<TextMeshProUGUI>().text = _currentWallet;

        // enable wallet info and disconnect button
    }

    private void DisableConnectWalletButton()
    {
        //old ui style
        _walletConnectButton.SetActive(false);

        // disable connect button
    }

    private void DisableWalletInfoButton()
    {
        //old ui style
        _walletConnectedButton.SetActive(false);


        // disable wallet info and disconnect button
    }

    private void ShowSendTXModal()
    {
    }

    private void DisableSendTXModal()
    {
    }
    #endregion

    private Sprite ToSprite(Texture2D texture)
    {
        if (texture == null) return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

    }

    private void DeleteAllChildObjects(Transform parent)
    {
        // Loop through each child of the parent GameObject
        foreach (Transform child in parent)
        {
            // Destroy each child GameObject
            Destroy(child.gameObject);
        }
    }
}





