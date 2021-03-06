﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrowdManager : MonoBehaviour {
	public int _spawnRange = 80;
	public Transform _spawnCenter;
	public Target[] _targets;

	public int[] targetTierOne;
	public int[] targetTierTwo;
	public int[] targetTierThree;

	[Header("Order: Assis - Nerds - Goths - Hippies")]
	public GameObject[] _followers;
	public int _totalAmount;

	[Space(5)]
	public int[] assisTierOne;
	public int[] assisTierTwo;
	public int[] assisTierThree;

	[Space(5)]
	public int[] nerdsTierOne;
	public int[] nerdsTierTwo;
	public int[] nerdsTierThree;

	[Space(5)]
	public int[] gothsTierOne;
	public int[] gothsTierTwo;
	public int[] gothsTierThree;

	[Space(5)]
	public int[] hippiesTierOne;
	public int[] hippiesTierTwo;
	public int[] hippiesTierThree;

	public GameObject[] convertParticles;

	private Transform _player;
	private List<Follower> _allFollower = new List<Follower>();
	private List<Follower> _activeFollower = new List<Follower>();

	private List<int[]> _tierOne = new List<int[]>();
	private List<int[]> _tierTwo = new List<int[]>();
	private List<int[]> _tierThree = new List<int[]>();

	private List<GameObject> _targetTierOne = new List<GameObject>();
	private List<GameObject> _targetTierTwo = new List<GameObject>();
	private List<GameObject> _targetTierThree = new List<GameObject>();

	private List<Follower> _activeAssis = new List<Follower>();
	private List<Follower> _activeNerds = new List<Follower>();
	private List<Follower> _activeGoths = new List<Follower>();
	private List<Follower> _activeHippies = new List<Follower>();

	private int _demandAssis;
	private int _demandNerds;
	private int _demandGoths;
	private int _demandHippies;

	private Dictionary<Target.Type, int> _demandCount = new Dictionary<Target.Type, int>();

	private MoshPit _firstPit;

	public class MoshPit{
		Vector3 _center;
		List<Follower> _pitPeople;
		List<Vector3> _path = new List<Vector3>();

		bool _started;

		public MoshPit(List<Follower> pitPeople){
			_pitPeople = pitPeople;
			_center = new Vector3(0, -9f, 0);
			this.Init();
		}

		public MoshPit(){
			_pitPeople = new List<Follower>();
			_path = new List<Vector3>();
			_center = new Vector3(-4.0f, -9f, -1.0f);
		}

		void Init(){
			int failed = 0;

			for (int i = 0; i < 10; i++){
				if(failed > 10)
					break;

				Vector3 pos = this.RandomCircle(_center, 4.0f, i);
				NavMeshHit hit;

				if(NavMesh.SamplePosition(pos, out hit, 2.0f, NavMesh.AllAreas)){
					_path.Add(hit.position);
				}else{
					i--;
					failed++;
					continue;
				}
			}
		}

		public void Start(){
			foreach(Follower follower in _pitPeople){
				follower.MoshPit(_path);
			}

			_started = true;
		}

		public void Stop(){
			if(_started){
				foreach(Follower follower in _pitPeople){
					follower.StopMoshPit();
				}
				_started = false;
			}
		}

		Vector3 RandomCircle (Vector3 center, float radius, int i){
			float ang = i * 360/10;
			Vector3 pos;
			pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
			pos.y = center.y;
			pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
			return pos;
     	}
	}
	public List<Follower> AllFollower{
		get { return _allFollower; }
	}
	public List<Follower> ActiveFollower{
		get { return _activeFollower; }
	}
	public int AssiCount{
		get { return _activeAssis.Count; }
	}
	public int HippieCount{
		get { return _activeHippies.Count; }
	}
	public int NerdCount{
		get { return _activeNerds.Count; }
	}
	public int GothCount{
		get { return _activeGoths.Count; }
	}
	// Use this for initialization
	void Awake (){
		_tierOne.Add(assisTierOne);
		_tierOne.Add(nerdsTierOne);
		_tierOne.Add(gothsTierOne);
		_tierOne.Add(hippiesTierOne);

		_tierTwo.Add(assisTierTwo);
		_tierTwo.Add(nerdsTierTwo);
		_tierTwo.Add(gothsTierTwo);
		_tierTwo.Add(hippiesTierTwo);

		_tierThree.Add(assisTierThree);
		_tierThree.Add(nerdsTierThree);
		_tierThree.Add(gothsTierThree);
		_tierThree.Add(hippiesTierThree);

		foreach(int i in targetTierOne)
			_targetTierOne.Add(_targets[i].gameObject);
		foreach(int i in targetTierTwo)
			_targetTierTwo.Add(_targets[i].gameObject);
		foreach(int i in targetTierThree)
			_targetTierThree.Add(_targets[i].gameObject);

		DeactivateAllDemandDirectionIndicator();
	}

	public void AddActiveFollower(Follower follower){
		_activeFollower.Add(follower);
		SetFollowerDemand(follower);
		AddType(follower);
	}

	public void RemoveActiveFollower(Follower follower){
		_activeFollower.Remove(follower);
		RemoveType(follower);
	}

	void AddType(Follower follower){
		switch(follower._type){
			case Follower.Type.assi:
				_activeAssis.Add(follower);
				CalculateDemandTier(_activeAssis, ref _demandAssis);
				break;
			case Follower.Type.nerd:
				_activeNerds.Add(follower);
				CalculateDemandTier(_activeNerds, ref _demandNerds);
				break;
			case Follower.Type.goth:
				_activeGoths.Add(follower);
				CalculateDemandTier(_activeGoths, ref _demandGoths);
				break;
			case Follower.Type.hippie:
				_activeHippies.Add(follower);
				CalculateDemandTier(_activeHippies, ref _demandHippies);
				break;
		}
	}

	void RemoveType(Follower follower){
		switch(follower._type){
			case Follower.Type.assi:
				_activeAssis.Remove(follower);
				CalculateDemandTier(_activeAssis, ref _demandAssis);
				break;
			case Follower.Type.nerd:
				_activeNerds.Remove(follower);
				CalculateDemandTier(_activeNerds, ref _demandNerds);
				break;
			case Follower.Type.goth:
				_activeGoths.Remove(follower);
				CalculateDemandTier(_activeGoths, ref _demandGoths);
				break;
			case Follower.Type.hippie:
				_activeHippies.Remove(follower);
				CalculateDemandTier(_activeHippies, ref _demandHippies);
				break;
		}
	}

	void CalculateDemandTier(List<Follower> active, ref int totalNumber){
		if(active.Count >= (_totalAmount/4)*0.5f){
			if(totalNumber != 2){
				totalNumber = 2;
				SwitchDemandTarget(active[0]._type);
				foreach(Follower act in active)
					act.SetDemand(2);
			}
		}
		else if(active.Count >= (_totalAmount/4)*1/3f){
			if(totalNumber != 1){
				if(totalNumber == 2)
					SwitchDemandTarget(active[0]._type);
				totalNumber = 1;
				foreach(Follower act in active)
					act.SetDemand(1);
			}
		}else{
			if(totalNumber != 0){
				if(totalNumber == 2)
					SwitchDemandTarget(active[0]._type);
				totalNumber = 0;
				foreach(Follower act in active)
					act.SetDemand(0);
			}
		}
	}

	void ActivateDemandDirectionIndicator(Target.Type type){
		foreach(Target tar in _targets){
			if(tar._type == type){
				tar.transform.Find("DirectionIndicator").gameObject.SetActive(true);
			}
		}
	}

	void DeactivateDemandDirectionIndicator(Target.Type type){
		foreach(Target tar in _targets){
			if(tar._type == type){
				tar.transform.Find("DirectionIndicator").gameObject.SetActive(false);
			}
				
		}
	}

	void DeactivateAllDemandDirectionIndicator(){
		foreach(Target tar in _targets){
			tar.transform.Find("DirectionIndicator").gameObject.SetActive(false);
		}
	}

	void SwitchDemandTarget(Follower.Type type){
		foreach(GameObject target in _targetTierTwo){
			foreach(Follower.Type objType in target.GetComponent<Target>()._followerLoveTypes){
				if(objType == type)
					target.SetActive(!target.activeSelf);
			}
		}
		foreach(GameObject target in _targetTierThree){
			foreach(Follower.Type objType in target.GetComponent<Target>()._followerLoveTypes){
				if(objType == type)
					target.SetActive(!target.activeSelf);
			}
		}
	}

	void SetFollowerDemand(Follower follower){
		switch(follower._type){
			case Follower.Type.assi:
				follower.SetDemand(_demandAssis);
				break;
			case Follower.Type.nerd:
				follower.SetDemand(_demandNerds);
				break;
			case Follower.Type.goth:
				follower.SetDemand(_demandGoths);
				break;
			case Follower.Type.hippie:
				follower.SetDemand(_demandHippies);
				break;
		}
	}

	public void AddDemandCount(Target.Type type){
		if(_demandCount.ContainsKey(type))
			_demandCount[type]++;
		else{
			ActivateDemandDirectionIndicator(type);
			_demandCount.Add(type, 1);
		}
	}

	public void RemoveDemandCount(Target.Type type){
		if(_demandCount.ContainsKey(type))
			if(_demandCount[type] - 1 <= 0){
				DeactivateDemandDirectionIndicator(type);
				_demandCount.Remove(type);
			}
			else{
				_demandCount[type]--;
			}
	}

	public void CreateNewActiveFollower(Follower.Type type, Vector3 pos, Leader leader){
		List<Target> tierOne = new List<Target>();
			List<Target> tierTwo = new List<Target>();
			List<Target> tierThree = new List<Target>();

			foreach(int x in _tierOne[(int)type]){
				tierOne.Add(_targets[x]);
			}
			foreach(int x in _tierTwo[(int)type]){
				tierTwo.Add(_targets[x]);
			}
			foreach(int x in _tierThree[(int)type]){
				tierThree.Add(_targets[x]);
			}

		GameObject follower = (GameObject)Instantiate(_followers[(int)type], pos, Quaternion.identity);
		follower.GetComponent<Follower>().SetPossibleTargets(tierOne, tierTwo, tierThree);
		follower.GetComponent<FollowTarget>().SetTarget(_player.transform);
		
		_allFollower.Add(follower.GetComponent<Follower>());

		follower.GetComponent<Follower>().Add(leader);

		if(convertParticles.Length > (int)type)
			Destroy(Instantiate(convertParticles[(int)type], pos, Quaternion.identity), 5f);
	}

	void Start () {
		_player = GameObject.FindGameObjectWithTag("Player").transform;

		for(int i = 0; i < _followers.Length; i++){
			List<Target> tierOne = new List<Target>();
			List<Target> tierTwo = new List<Target>();
			List<Target> tierThree = new List<Target>();

			foreach(int x in _tierOne[i]){
				tierOne.Add(_targets[x]);
			}
			foreach(int x in _tierTwo[i]){
				tierTwo.Add(_targets[x]);
			}
			foreach(int x in _tierThree[i]){
				tierThree.Add(_targets[x]);
			}

			for(int j = 0; j < _totalAmount/4; j++){
				NavMeshHit hit;
				Vector3 randomPosition = new Vector3(Random.Range(_spawnCenter.position.x -_spawnRange, _spawnCenter.position.x + _spawnRange + 1), Random.Range(_spawnCenter.position.y - 25, _spawnCenter.position.y + 12), Random.Range(_spawnCenter.position.z -_spawnRange, _spawnCenter.position.z + _spawnRange + 1));
				if(NavMesh.SamplePosition(randomPosition, out hit, 10.0f, NavMesh.AllAreas)){
					randomPosition = hit.position;
					Ray ray = new Ray(hit.position + Vector3.up * 100, hit.position - Vector3.up * 100);
					RaycastHit raycastHit;
					if(Physics.Raycast(ray, out raycastHit, 200f, LayerMask.GetMask("Terrain")))
						randomPosition = raycastHit.point;
					else {
							j--;
						continue;
						}
				}else{
					j--;
					continue;
				}

				GameObject follower = (GameObject)Instantiate(_followers[i], randomPosition, Quaternion.identity);
				follower.GetComponent<Follower>().SetPossibleTargets(tierOne, tierTwo, tierThree);
				follower.GetComponent<FollowTarget>().SetTarget(_player.transform);

				_allFollower.Add(follower.GetComponent<Follower>());
			}
		}

		List<Follower> pitPeople = new List<Follower>();

		pitPeople = _allFollower;

		_firstPit = new MoshPit(pitPeople);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.N))
			_firstPit.Start();
		if(Input.GetKeyDown(KeyCode.B))
			_firstPit.Stop();
	}
}
