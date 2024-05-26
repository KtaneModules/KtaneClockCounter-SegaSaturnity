using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClockCounter : MonoBehaviour {

	public KMBombInfo bomb;
	public KMBombModule module;
	public KMAudio audio;
	
	public KMSelectable[] buttons;
	public Material[] arrows;
	public GameObject GG;
	
	private static int moduleIdCounter = 1;
	private int moduleId;
	
    private static readonly int[][] _arrowlist = new int[4][]
    {
        new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 2, 4, 12, 14, 15 },
		new int[]{ 5, 10, 9, 16, 11, 13, 17, 15, 13, 18, 12, 19, 8, 20, 11, 18 },
		new int[]{ 23, 24, 2, 21, 3, 16, 8, 17, 1, 9, 5, 20, 20, 16, 15, 5 },
		new int[]{ 17, 21, 10, 19, 2, 25, 26, 23, 19, 16, 10, 22, 3, 20, 18, 14 }
    };
	
	private int[] answer = new int[10];
	private int[] og_ans = new int[10];
	private int[] arr_in = new int[10];
	
	private static string[] str_arr = new string[]{ "⤾","⤦","↬","↷","⥁","↶","⤵","⤷","⟳","↫","⟲","⤸","		\u20D4","		\u20D5","⤹","⤺","⤳","⤥","⤶","⤿","⤣","⤤","⥀","⬿","⤻","⤴","⤾" };
	
	private int count;
	private int clock;

	private string input;

	void Start () {
		Debug.LogFormat("[↻↺ #{0}] Starting up module", moduleId);
		
		generateBoard();
	}
	
	void Awake () {
		moduleId = moduleIdCounter++;
		
		for (int i = 0; i < 10; i++){
			int j = i;
			buttons[j].OnInteract += delegate () { checkPos(j); return false; };
		}
		
	}
	
	void generateBoard(){
		bool clockwise = UnityEngine.Random.Range(0, 2) == 1;
		int i, a, loop;
		i = a = loop = 0;
		
		int sel = UnityEngine.Random.Range(0, 15);
		answer[a] = _arrowlist[0][sel];
		while(answer[9] == 0 && loop < 100){
			a++;
			loop++;
			if (clockwise) {
				i++;
				if (i > 3) i = 0;
				
				if (i == 1 || i == 3){
					sel = (sel/4) * 4 + UnityEngine.Random.Range(0, 3);
				} else {
					sel = UnityEngine.Random.Range(0, 3) * 4 + (sel%4);
				}
				if (answer.Contains(_arrowlist[i][sel])){
					a--;
					i--;
					continue;
				}
			} else {
				i--;
				if (i < 0) i = 3;
				
				if (i == 0 || i == 2){
					sel = (sel/4) * 4 + UnityEngine.Random.Range(0, 3);
				} else {
					sel = UnityEngine.Random.Range(0, 3) * 4 + (sel%4);
				}
				if (answer.Contains(_arrowlist[i][sel])){
					a--;
					i++;
					continue;
				}
			}
			answer[a] = _arrowlist[i][sel];
		}
		if (loop > 99){
			generateBoard();
			return;
		}
		if ( clockwise != true ) System.Array.Reverse(answer);
		
		og_ans = (int[])answer.Clone();
		for (int t = 0; t < answer.Length; t++ )
        {
            int tmp = answer[t];
            int r = Random.Range(t, answer.Length);
            answer[t] = answer[r];
            answer[r] = tmp;
        }
	
		Debug.LogFormat("[↻↺ #{0}] Shuffled display: {1}", moduleId, convert_str(answer));
		Debug.LogFormat("[↻↺ #{0}] Valid solution: {1}", moduleId, convert_str(og_ans));
		reset();
	}
	
	void reset(){
		input = "";
		count = clock = 0;
		for (int b = 0; b < 10; b++){
			buttons[b].GetComponent<MeshRenderer>().sharedMaterial = arrows[answer[b]-1];
		}
		
		arr_in = new int[10];
	}
	
	void checkPos(int pos){
		if (buttons[pos].GetComponent<MeshRenderer>().sharedMaterial == arrows[27]) return;
		buttons[pos].GetComponent<MeshRenderer>().sharedMaterial = arrows[27];
		
		buttons[pos].AddInteractionPunch(0.25f);
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		input += str_arr[answer[pos]-1];
		arr_in[count] = answer[pos];
		
		count++;
		if (count > 1) {	
			for (int j = 0; j < 2; j++){
				bool fail = false;
				for (int a = 0; a < 10 && fail == false; a++){
					if (arr_in[a] != 0)	fail = true;
					for (int x = 0; x < _arrowlist.Length; x += 1) {
						for (int y = 0; y < _arrowlist[x].Length; y += 1) {
							if (_arrowlist[x][y] == arr_in[a] && check(x, y, a) == true) {
								fail = false;
							}
						}
					}
				}
				
				System.Array.Reverse(arr_in);
				if (fail == false) {
					if (count == 10) {
						Debug.LogFormat("[↻↺ #{0}] Solved! Answer input: {1}", moduleId, input);
						audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
						StartCoroutine(Animation());
						module.HandlePass();
					}
					if (j == 0) System.Array.Reverse(arr_in);
					return;
				}
			}
			
			Debug.LogFormat("[↻↺ #{0}] Strike! Answer input: {1}", moduleId, input);
			reset();
			module.HandleStrike();
		}
	}
	
	bool check(int x, int y, int pos_i){
		bool[] valid = new bool[] {false, false};
		if (pos_i + 1 > 9 || arr_in[pos_i+1] == 0) valid[1] = true;
		if (pos_i - 1 < 0 || arr_in[pos_i-1] == 0) valid[0] = true;

		//checking clockwise
		if ((x == 0 || x == 2)){
			//check horizontal and vertical
			for (int i = 0; i < 4; i ++){
				if (valid[1] != true && _arrowlist[mod(x+1, 4)][(y/4) * 4 + i] == arr_in[pos_i+1]) {
					valid[1] = true;
				}
				if (valid[0] != true && _arrowlist[mod(x-1, 4)][i * 4 + (y%4)] == arr_in[pos_i-1]) {
					valid[0] = true;
				}
			}
			
		} else if (clock != -1) {
			//check vertical and horizontal
			for (int i = 0; i < 4; i ++){
				if (valid[0] != true && _arrowlist[mod(x-1, 4)][(y/4) * 4 + i] == arr_in[pos_i-1]) {
					valid[0] = true;
				}
				if (valid[1] != true && _arrowlist[mod(x+1, 4)][i * 4 + (y%4)] == arr_in[pos_i+1]) {
					valid[1] = true;
				}
			}
		}
		
		if (valid[0] == true && valid[1] == true) return true;
		return false;
	}
	
	private IEnumerator Animation()
    {
		bool playing = true;
		float acol = 0;
        while (playing)
        {
			if (acol < 1f) {
				acol += 0.1f;
				for (int i=0; i< GG.transform.childCount; i++) {
					GG.transform.GetChild(i).GetComponent<TextMesh>().color = new Color(1,1,1,acol);
				}
			}
			GG.transform.Rotate(new Vector3(0, 5f, 0));
            yield return new WaitForSeconds(.03f);
        }
    }
 
 	string convert_str(int[] arr){
		string result = "";
		foreach (int n in arr) {
			result += str_arr[n-1];
		}
		return result;
	}
	
	int mod(int a, int b) {  return ((a %= b) < 0) ? a+b : a;  }

#pragma warning disable IDE0051 // Remove unused private members
    readonly string TwitchHelpMessage = "\"!{0} press 1 2 3 4 5 6 7 8 9 10\" [Presses the 1st, 2nd, 3rd, 4th, 5th, 6th, 7th, 8th, 9th, 10th buttons in column reading order of the module. (TOP TO BOTTOM, THEN LEFT TO RIGHT) Chain presses with spaces.]";
#pragma warning restore IDE0051 // Remove unused private members

    IEnumerator ProcessTwitchCommand(string cmd)
    {
		var intCmd = cmd.ToLowerInvariant().Split();
		if (intCmd.Any())
        {
			if (intCmd.First() != "press")
            {
				yield return "sendtochaterror Button presses for this module must start with \"press\". No button presses will be accepted without it.";
				yield break;
            }
			var allIdxesToPress = new List<int>();
			foreach (var possibleNum in intCmd.Skip(1))
            {
				int value;
				if (!int.TryParse(possibleNum, out value) || value < 1 || value > 10)
                {
					yield return "sendtochaterror \"" + possibleNum + "\" was not a valid number to press. Stopping here.";
					yield break;
				}
				allIdxesToPress.Add(value - 1);
            }
			if (!allIdxesToPress.Any())
            {
				yield return "sendtochaterror No buttons were specified! Did you forget to insert any numbers?";
				yield break;
			}
			yield return null;
			foreach (var idx in allIdxesToPress)
            {
				buttons[idx].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
        }
		yield break;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		if (count > 0 && !arr_in.Take(count).SequenceEqual(og_ans.Take(count)))
			reset();
		var orderAnsIdx = Enumerable.Range(0, 10).Select(a => answer.IndexOf(b => b == og_ans[a]));
		foreach (var idx in orderAnsIdx)
        {
			buttons[idx].OnInteract();
			yield return new WaitForSeconds(0.1f);
		}
    }
}
