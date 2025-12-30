using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using SimpleJSON; // ✅ SimpleJSON 라이브러리 사용 (JSON 파싱용)

/*
 * GPTDialogueManager
 * ────────────────────────────────
 * Unity UI에서 사용자의 입력을 받아 OpenAI GPT API로 전송하고,
 * GPT의 응답을 NPC 대화창(TextMeshPro)에 표시하는 매니저 스크립트입니다.
 */

public class GPTDialogueManager : MonoBehaviour
{
    [Header("UI 연결")] // Inspector 상에서 UI 오브젝트 연결 구역 표시
    public TMP_InputField userInput;      // 사용자의 입력창 (TextMeshPro InputField)
    public TextMeshProUGUI npcText;       // GPT의 응답을 표시할 NPC 말풍선 텍스트
    public Button sendButton;             // "SEND" 버튼

    // API 키를 보관하는 문자열 (Inspector에서 입력 가능)
    // [Header("API 설정")] [TextArea(2,5)] 부분은 주석 처리되어 있으므로 단순 문자열 필드로 사용
    public string openaiApiKey = "API키를 붙여넣기해주세요."; // ⚠️ 실제 OpenAI API 키 (개인 계정 키를 여기에 넣음)
    
    private bool isProcessing = false; // 중복 요청 방지를 위한 플래그

    void Start()
    {
        // ▶ 버튼 클릭 시 OnSendMessage() 메서드 실행
        sendButton.onClick.AddListener(OnSendMessage);

        // 초기 대사 설정
        if (npcText != null)
            npcText.text = "What Can I Do for you?";
    }

    // 사용자가 버튼을 눌렀을 때 호출되는 메서드
    public void OnSendMessage()
    {
        if (isProcessing) return; // 요청 처리 중이면 중복 실행 방지

        string message = userInput.text.Trim(); // 입력된 문장 가져오기 (양끝 공백 제거)

        // 입력이 비어 있으면 경고 메시지 표시
        if (string.IsNullOrEmpty(message))
        {
            npcText.text = "⚠️ ??? ";
            return;
        }

        // GPT 응답 대기 중 텍스트 출력
        npcText.text = "💭 ...Thinking...";

        // GPT 요청 코루틴 실행
        StartCoroutine(SendToGPT(message));
    }

    // GPT API로 사용자 메시지를 전송하는 코루틴
    IEnumerator SendToGPT(string userMessage)
    {
        isProcessing = true; // 요청 중 상태로 전환

        // ✅ OpenAI Chat Completion API Endpoint
        string endpoint = "https://api.openai.com/v1/chat/completions";

        // ✅ 요청 JSON Body 구성
        // model: 사용할 GPT 모델 (gpt-4o)
        // messages: 대화 내용 배열
        string jsonBody = "{\"model\": \"gpt-4o\", \"messages\": [{\"role\": \"user\", \"content\": \"" 
                          + EscapeJson(userMessage) + "\"}]}";

        // ✅ UnityWebRequest 객체 생성 (POST 요청)
        using (UnityWebRequest req = new UnityWebRequest(endpoint, "POST"))
        {
            // 요청 Body를 UTF-8로 인코딩하여 전송 준비
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer(); // 응답 받을 버퍼 생성
            req.SetRequestHeader("Content-Type", "application/json"); // JSON 형식 명시
            req.SetRequestHeader("Authorization", "Bearer " + openaiApiKey); // OpenAI 인증 헤더

            // API 응답 대기 (비동기)
            yield return req.SendWebRequest();

            // ✅ 요청 성공 시
            if (req.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // SimpleJSON을 사용해 응답 파싱
                    var json = JSON.Parse(req.downloadHandler.text);

                    // GPT 응답 내용 추출
                    string content = json["choices"][0]["message"]["content"];

                    // 말풍선에 표시
                    npcText.text = content.Trim();

                    // 콘솔에도 출력
                    Debug.Log("✅ GPT: " + content);
                }
                catch (System.Exception e)
                {
                    // 파싱 오류 발생 시
                    npcText.text = "⚠️ error : " + e.Message;
                    Debug.LogError("Parse Error: " + e.Message + "\n" + req.downloadHandler.text);
                }
            }
            else // ❌ 요청 실패 시
            {
                npcText.text = "❌ 요청 실패: " + req.error;
                Debug.LogError("HTTP Error: " + req.error + "\n" + req.downloadHandler.text);
            }
        }

        // 처리 완료 후 플래그 해제
        isProcessing = false;
    }

    // JSON 문자열 안에서 따옴표(")나 역슬래시(\)를 이스케이프 처리
    private string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}