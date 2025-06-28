using System.Collections;
using TMPro;
using UnityEngine;

public class BattleScore_UI : Base_UI
{
    [SerializeField] TextMeshProUGUI _addScoreText;
    [SerializeField] TextMeshProUGUI _scoreText;
    [SerializeField] float _fadeTime = 0.2f;
    [SerializeField] float _displayTime = 0.5f;
    [SerializeField] float _scoreUpdateFrame = 60;
    WaitForSecondsRealtime _displayWait;

    int _totalScore = 0;
    int _currentDisplayScore = 0;

    Coroutine _fadeCoroutine;
    Coroutine _updateTotalScoreCoroutine;

    public override void Initialize()
    {
        base.Initialize();
        _displayWait = new WaitForSecondsRealtime(_displayTime);
        _addScoreText.color = Color.clear;

        _scoreUpdateFrame = Application.targetFrameRate;

        if (_scoreUpdateFrame <= 0) // Application.targetFrameRate이 설정되어 있지 않으면 -1
            _scoreUpdateFrame = 60;
    }

    public void OnGainScore(int score)
    {
        _addScoreText.text = score >= 0 ? $"+{score}" : score.ToString();
        _totalScore += score;
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(Fade());
        UpdateTotalScore(_totalScore);
    }

    public void UpdateTotalScore(int totalScore)
    {
        _totalScore = totalScore;

        if (_updateTotalScoreCoroutine != null)
            StopCoroutine(_updateTotalScoreCoroutine);
        _updateTotalScoreCoroutine = StartCoroutine(UpdateTotalScoreCoroutine());
    }

    IEnumerator Fade()
    {
        _addScoreText.color = Color.clear;

        float elapsedTime = 0f;

        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / _fadeTime);
            _addScoreText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        yield return _displayWait;

        elapsedTime = 0f;
        while (elapsedTime < _fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / _fadeTime));
            _addScoreText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        _fadeCoroutine = null;
    }

    IEnumerator UpdateTotalScoreCoroutine()
    {
        int scoreUpdateAmountForOneFrame = Mathf.CeilToInt((_totalScore - _currentDisplayScore) / _scoreUpdateFrame);
        scoreUpdateAmountForOneFrame = Mathf.Max(scoreUpdateAmountForOneFrame, 7);

        while (_currentDisplayScore + scoreUpdateAmountForOneFrame < _totalScore)
        {
            _currentDisplayScore += scoreUpdateAmountForOneFrame;
            _scoreText.text = _currentDisplayScore.ToString();
            yield return null;
        }
        _currentDisplayScore = _totalScore;
        _scoreText.text = _currentDisplayScore.ToString();

        _updateTotalScoreCoroutine = null;
    }
}
