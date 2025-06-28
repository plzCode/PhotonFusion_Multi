using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LobbySceneCamController : MonoBehaviour
{
    public static LobbySceneCamController Instance { get; private set; }

    [SerializeField] float _transitionDuration = 0.5f;
    public float TransitionDuration
    {
        get { return _transitionDuration; }
        set {
            _transitionDuration = value;
            _waitForTransition = new WaitForSecondsRealtime(_transitionDuration);
        }
    }
    public EScreenType CurrentScreenType { get; private set; } = EScreenType.MainMenu;
    
    [SerializeField] UniversalRendererData _lobbySceneRD;

    [Space(5)]
    [Header("Camera Transform")]
    [SerializeField] Transform _mainMenuCamTrans;
    [SerializeField] Transform _playModeCamTrans;
    [SerializeField] Transform _characterSelectCamTrans;
    [SerializeField] Transform _roomCamTrans;

    readonly string RetroRFName = "Retro";
    readonly string SnowNoiseRFName = "SnowNoise";
    ScriptableRendererFeature _retroRF;
    ScriptableRendererFeature _snowNoiseRF;

    bool _isInitialized = false;
    WaitForSecondsRealtime _waitForTransition;
    Coroutine _changeScreenEffectCoroutine;

    #region Event Handlers
    //public void OnClickMainMenu() => OnChangeScreen(EScreenType.MainMenu);
    //public void OnClickPlaymode() => OnChangeScreen(EScreenType.Playmode);
    //public void OnClickCreateRoom() => OnChangeScreen(EScreenType.CreateRoom);
    //public void OnClickJoinRoom() => OnChangeScreen(EScreenType.JoinRoom);
    //public void OnClickCharacterSelect() => OnChangeScreen(EScreenType.CharacterSelection);
    //public void OnClickRoom() => OnChangeScreen(EScreenType.Room);
    #endregion

    void Awake()
    {
        transform.position = _mainMenuCamTrans.position;
        transform.rotation = _mainMenuCamTrans.rotation;

        if (!_isInitialized)
            Initialize();

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnChangeScreen(EScreenType menuType)
    {
        if (_lobbySceneRD == null)
            return;

        if (!_isInitialized)
        {
            Initialize();
            _isInitialized = true;
        }

        CurrentScreenType = menuType;
        ExecuteChangeScreenEffect(menuType);
    }

    public void ExecuteChangeScreenEffect(EScreenType type)
    {
        if (!_isInitialized)
            Initialize();

        if (IsSameScreen(type))
            return;

        if (_changeScreenEffectCoroutine != null)
            StopCoroutine(_changeScreenEffectCoroutine);

        _changeScreenEffectCoroutine = StartCoroutine(ChangeScreenEffect());
        CurrentScreenType = type;

        switch (CurrentScreenType)
        {
            case EScreenType.MainMenu:
                transform.position = _mainMenuCamTrans.position;
                transform.rotation = _mainMenuCamTrans.rotation;
                break;
            case EScreenType.Playmode:
            case EScreenType.CreateRoom:
            case EScreenType.JoinRoom:
                transform.position = _playModeCamTrans.position;
                transform.rotation = _playModeCamTrans.rotation;
                break;
            case EScreenType.CharacterSelection:
                transform.position = _characterSelectCamTrans.position;
                transform.rotation = _characterSelectCamTrans.rotation;
                break;
            case EScreenType.Room:
                transform.position = _roomCamTrans.position;
                transform.rotation = _roomCamTrans.rotation;
                break;
        }

    }

    IEnumerator ChangeScreenEffect()
    {
        if (_waitForTransition == null)
            _waitForTransition = new WaitForSecondsRealtime(TransitionDuration);

        _retroRF.SetActive(false);
        _snowNoiseRF.SetActive(true);
        yield return _waitForTransition;
        _retroRF.SetActive(true);
        _snowNoiseRF.SetActive(false);
    }

    void Initialize()
    {
        if (_lobbySceneRD == null)
        {
            Debug.LogError("Renderer Data is not assigned in LobbySceneCamController.");
            return;
        }

        foreach (var feature in _lobbySceneRD.rendererFeatures)
        {
            if (feature.name == RetroRFName)
            {
                _retroRF = feature;
            }
            else if (feature.name == SnowNoiseRFName)
            {
                _snowNoiseRF = feature;
            }
        }

        _retroRF.SetActive(true);
        _snowNoiseRF.SetActive(false);

        _isInitialized = true;
    }

    bool IsSameScreen(EScreenType type)
    {
        switch (type)
        {
            case EScreenType.MainMenu:
                if (CurrentScreenType == EScreenType.MainMenu)
                    return true;
                break;
            case EScreenType.Playmode:
            case EScreenType.CreateRoom:
            case EScreenType.JoinRoom:
                if (CurrentScreenType == EScreenType.Playmode ||
                    CurrentScreenType == EScreenType.CreateRoom ||
                    CurrentScreenType == EScreenType.JoinRoom)
                    return true;
                break;
            case EScreenType.CharacterSelection:
                if (CurrentScreenType == EScreenType.CharacterSelection)
                    return true;
                break;
            case EScreenType.Room:
                if (CurrentScreenType == EScreenType.Room)
                    return true;
                break;
        }


        return false;
    }

    public void RemoveRetroEffect()
    {
        Camera camera = GetComponent<Camera>();

        var additionalData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (additionalData == null)
        {
            Debug.LogWarning("UniversalAdditionalCameraData가 없습니다.");
            return;
        }

        // UniversalRenderPipelineAsset에서 Renderer 인덱스를 찾아야 합니다.
        // 일반적으로 0번이 기본 Renderer입니다.
        // 만약 _normalRD가 몇 번째 Renderer인지 알아야 한다면, 프로젝트 세팅에서 확인 필요
        additionalData.SetRenderer(0); // 0은 기본 Renderer 인덱스 예시
    }
    void OnApplicationQuit()
    {
        _retroRF.SetActive(true);
        _snowNoiseRF.SetActive(false);
    }
}

public enum EScreenType
{
    MainMenu = 0,
    Playmode = 1,
    CreateRoom = 2,
    JoinRoom = 3,
    CharacterSelection = 4,
    Room = 5,
}