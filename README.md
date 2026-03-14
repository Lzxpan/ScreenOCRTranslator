# ScreenOCRTranslator<br>
[![ScreenTranslator使用教學](https://img.youtube.com/vi/OUdjk_U1lEE/0.jpg)](https://www.youtube.com/watch?v=OUdjk_U1lEE)<br>

![畫面預覽](images/main.jpg)<br>
![Tokens用量](images/Tokens.jpg)<br>
這款工具可以用來翻譯遊戲畫面或其他應用程式中的文字，非常方便。<br>

使用說明<br>

取得 API Key：可依你要用的引擎分別申請（Gemini / Mistral / Groq；Pixtral-12B-2409 採 vLLM 路線）。<br>
設定軟體：將對應 API Key 貼入各自欄位，並選擇模型。<br>
進行翻譯：按下鍵盤 Q 鍵 + 滑鼠左鍵，框選畫面中想要翻譯的區域。<br>
完成翻譯：系統會自動辨識並翻譯為繁體中文，並將翻譯後文字覆蓋在原始位置上。<br>
配額與用量：可點「今日引擎使用量」查看當日各模型的成功次數、失敗次數、消耗 Tokens、上限與 RPM。<br>
版本說明<br>
V0.95b<br>

新增多引擎自動切換機制：Gemini → Pixtral-12B-2409(vLLM) → Mistral Pixtral → Groq Llama4。<br>
當遇到配額或速率限制（例如 429）時，會自動切換到下一組可用 API Key。<br>
全部引擎都無法使用時，會顯示「所有API KEY額度已用盡」。<br>
新增 Pixtral / Mistral Pixtral / Llama4 的 API Key 與模型欄位，並可分別儲存。<br>
新增各 API key 申請連結（Gemini、Mistral、Groq）與 Pixtral-12B-2409 路線說明連結。<br>
新增關閉視窗提示：可選擇「關閉程式」、「縮小到右下角常駐」或「取消」。<br>
新增「今日引擎使用量」面板：顯示每個引擎/模型的成功請求、失敗計數、Tokens 用量、上限與 RPM，並每日自動更新。<br>
配額統計改為「成功請求為主、失敗另列計數」，便於判讀實際消耗。<br>

V0.94b<br>
1.送出 AI 翻譯請求時，會在滑鼠游標旁顯示「翻譯中...」。<br>
2.翻譯失敗時，會在滑鼠游標旁顯示「翻譯失敗」，2 秒後自動恢復。<br>
3.框選視窗維持灰色半透明遮罩，並讓框選邊框（淺藍外框＋紅色內框）以實色清楚顯示。<br>
4.支援縮小到右下角系統匣常駐，可從系統匣快速還原或結束程式。<br>

V0.93b<br>
1.降低圖片大小以降低Token使用量。<br>
2.提高OCR辨識率。<br>
3.顯示翻譯時間可設定，在顯示區域按滑鼠右鍵可立即取消顯示。<br>
4.增加Gemini API key申請連結。<br>
5.增加使用說明，Tokens消耗量，錯誤說明。<br>

V0.92b<br>
1.新增字型大小可隨著擷取框大小自動變化。<br>
2.拿掉Gemini 2.0系列模型，保留2.5與3.0，無法使用所以移除。<br>

V0.91b<br>
1.移除程式開啟時焦點停留在API key處，避免誤觸清空API key。<br>
2.新增雙螢幕擷取功能。<br>

V0.9b<br>
1.第一版測試版。<br>
