using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockButton : Interactable {


	public virtual void OnClick(int option){
		if (option == 0)
			selected.rocks++;
	}
}
