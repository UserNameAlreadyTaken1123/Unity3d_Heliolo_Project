using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Luminosity.IO;

public class Tutorial : MonoBehaviour {

	public GameObject player;
	public CycleSwords cycleSwords;
	public CycleGuns cycleGuns;
	public Third_Person_Camera cameraScript;
	public List<GameObject> pickableSwords;
	public List<GameObject> pickableGuns;
	public List<GameObject> enemies;
	public int index;

	public GameObject sword1;
	public GameObject sword2;
	public GameObject sword3;
	public GameObject sword4;
	public GameObject sword5;

	public GameObject gun;

	public GameObject enemy1;
	public GameObject enemy2;
	public GameObject enemy3;
	public GameObject enemy4;
	public GameObject enemy5;
	public GameObject enemy6;

	public int progress = 1;
	public float timer;
	public int counter;

	// Use this for initialization
	void Start () {
		if (sword1)
			pickableSwords.Add (sword1);
		if (sword2)
			pickableSwords.Add (sword2);
		if (sword3)
			pickableSwords.Add (sword3);
		if (sword4)
			pickableSwords.Add (sword4);
		if (sword5)
			pickableSwords.Add (sword5);

		if (enemy1)
			enemies.Add (enemy1);
		if (enemy2)
			enemies.Add (enemy2);
		if (enemy3)
			enemies.Add (enemy3);
		if (enemy4)
			enemies.Add (enemy4);
		if (enemy5)
			enemies.Add (enemy6);

		foreach (GameObject sword in pickableSwords)
			sword.SetActive(false);
		foreach (GameObject gun in pickableGuns)
			gun.SetActive(false);
		foreach (GameObject enemy in enemies)
			enemy.SetActive(false);		
	}
	
	// Update is called once per frame
	void Update () {
		if (!player && GameObject.Find ("Player")) {
			player = GameObject.Find ("Player");
			cycleSwords = player.GetComponent<CycleSwords> ();
			cycleGuns = player.GetComponent<CycleGuns> ();
			cameraScript = Camera.main.GetComponent<Third_Person_Camera> ();
		}
		
		switch (progress) {
		case 1:
			if (InputManager.GetButtonDown ("Magic")) {
				progress = 2;
			}
			break;

		case 2:
			if (InputManager.GetAxis ("Vertical") != 0f || InputManager.GetAxis ("Horizontal") != 0f)
				timer = timer + Time.deltaTime;
			if (timer >= 5f) {
				timer = 0f;
				progress = 3;
			}
			break;

		case 3:
			if (InputManager.GetAxis ("Mouse X") != 0f || InputManager.GetAxis ("Mouse Y") != 0f || InputManager.GetAxis ("RightAnalogX") != 0f || InputManager.GetAxis ("RightAnalogY") != 0f ) {
				timer = timer + Time.deltaTime;
				if (InputManager.GetAxis ("Vertical") != 0f || InputManager.GetAxis ("Horizontal") != 0f)
					timer = timer + Time.deltaTime;
			}
			if (timer >= 7.5f) {
				timer = 0f;
				progress = 4;
			}
			break;

		case 4:
			if (InputManager.GetButtonDown ("Reset Camera"))
				counter = counter + 1;
			
			if (counter == 3) {
				counter = 0;
				progress = 5;
			}
			break;

		case 5:
			if (counter == 0 && cameraScript.distance == cameraScript.distanceMin)
				counter = counter + 1;
			if (counter == 1 && cameraScript.distance == cameraScript.distanceMax)
				counter = counter + 1;

			if (counter == 2) {
				counter = 0;
				progress = 6;
			}
			break;

		case 6:
			if (cameraScript.distance == 5f)
				progress = 7;
			break;

		case 7:
			if (counter == 0) {
				foreach (GameObject sword in pickableSwords) {
					sword.SetActive (true);
				}
				counter = 1;
			}

			for (var index = pickableSwords.Count - 1; index > -1; index--) {
				if (pickableSwords [index] == null)
					pickableSwords.RemoveAt (index);
			}

			if (pickableSwords.Count == 0) {
				counter = 0;
				progress = 8;
			}

			break;

		case 8:
			if (counter == 0 && cycleSwords.slot2)
				counter = 1;
			if (counter == 1 && InputManager.GetAxisRaw ("Direction Vertical") < 0.0f){
				counter = 0;
				progress = 9;
			}
			break;

		case 9:
			if (InputManager.GetAxisRaw ("Direction Vertical") > 0.0f){
				progress = 10;			
			}

			break;

		case 10:
			if (InputManager.GetButtonDown ("Magic")) {
				progress = 11;
			}
			break;

		case 11:
			enemy1.SetActive (true);
			if (enemy1.GetComponent<HealthBar> ().isDead)
				progress = 12;
			break;

		case 12:
			if (counter == 0 && player.GetComponent<Hero_Movement> ().isBouncing)
				counter = counter + 1;
			if (counter == 1 && !player.GetComponent<Hero_Movement> ().isBouncing)
				counter = counter + 1;

			if (counter == 2 && player.GetComponent<Hero_Movement> ().isBouncing)
				counter = counter + 1;
			if (counter == 3 && !player.GetComponent<Hero_Movement> ().isBouncing)
				counter = counter + 1;

			if (counter == 4 && player.GetComponent<Hero_Movement> ().isBouncing)
				counter = counter + 1;
			if (counter == 5 && !player.GetComponent<Hero_Movement> ().isBouncing)
				counter = counter + 1;

			if (counter == 5) {
				counter = 0;
				progress = 13;
			}
			break;

		case 13:
			if (counter == 0) {
				GameObject pickupThis = Instantiate (gun, player.transform.position, player.transform.rotation);
				pickupThis.GetComponent<CapsuleCollider> ().radius = 100f;
				counter = 1;
			}

			if (cycleGuns.currentGun == cycleGuns.slot2) {
				counter = 0;
				progress = 14;
			}
			break;

		case 14:
			enemy2.SetActive (true);
			enemy2.GetComponent<AI> ().doNotMove = true;
			if (enemy2.GetComponent<HealthBar> ().isDead)
				progress = 15;
			break;

		case 15:
			if (InputManager.GetButtonDown ("Magic")) {
				SceneManager.LoadScene ("NewMainMenuGeneric");
			}
			break;
		}
	}

	void OnGUI () {

		if (GUI.Button (new Rect (Screen.width - 60, 10, 50, 30), "Skip")) {
			progress = progress + 1;
			counter = 0;
			timer = 0;
		}

		switch (progress) {
		case 1:
			GUI.skin.box.alignment = TextAnchor.UpperLeft;
			GUI.skin.box.wordWrap = true;

			GUI.Box (new Rect (10, 70, 200, 275),	"Hello, welcome to the tutoriel!\r\n " +
			"Just because I know you hate English, I'll proced to speak in spanish!\r\n " +
			"... \r\n" +
			"Bueno, bienvenido al tutorial. Le puse mucha onda y mucho amor a esto, \r\n" +
			"así que por lo menos ponele onda.\r\n" +
			"\r\n" +
			"\r\n" +
			"\r\n" +
			"Oprime 'Usar' (Por defecto F o Círculo) para continuar\r\n");
			break;
		case 2:
			GUI.Box (new Rect (10, 70, 200, 275),	"Bueno, por ahora nada nuevo. " +
			"Te movés con W, A, S, D o con el Stick Analógico del Joystick\r\n " +
			"... \r\n" +
			"Se salta con  Joystick X o con Barra Espaciadora, \r\n" +
			"y se corre más rapido manteniendo presionado el Stick Analógico Izquierdo o manteniendo presionado Shift.\r\n" +
			"\r\n" +
			"\r\n" +
			"\r\n" +
			"Da un par de vueltas por el detallado escenario para continuar\r\n");
			break;
		case 3:
			GUI.Box (new Rect (10, 70, 200, 275),
				"La cámara se mueve mediante el Mouse o el Stick Analógico derecho\r\n " +
				"\r\n" +
				"\r\n" +
				"\r\n" +
				"mueve un rato la cámara mientras caminas para continuar\r\n");
			break;
		case 4:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Si estas en un apuro y rapidamente necesitas ver lo que hay en frente de tu personaje,\r\n " +
				"por lo que rotar la camara podría suponer una pérdida de tiempo, puedes oprimir\r\n " +
				"'Reset Camera' (Por defecto V o Presionando el Stick A. Derecho) para reiniciar la cámara.\r\n " +
				"\r\n" +
				"\r\n" +
				"\r\n" +
				"Rota tu personaje y reinicia la cámara tres veces para continuar.\r\n");
			break;

		case 5:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Dependiendo del entorno en que te encuentres, si se trata de un campo abierto o " +
				"de un pasillo, puedes manejar la distancia de la cámara respecto del personaje.\r\n " +
				"\r\n" +
				"Con mouse no hay mucha ciencia, solo es necesario usar el Scroll Wheel (ruedita).\r\n " +
				"En el caso del Joystick es necesario mantener el Stick A. Derecho e inclinar el stick hacia delante o hacia atrás.\r\n " +
				"\r\n" +
				"Acerca la cámara todo lo que puedas y luego aléjala lo mas lejos posible para continuar.\r\n");
			break;

		case 6:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Como ya sabes, puedes reiniciar la cámara con 'Reset Camera' (V o presionar el S.A.Derecho),\r\n " +
				"pero si quieres reiniciar la distancia de la cámara, tan solo presiona dos veces consecutivas\r\n " +
				"'Reset Camera' (V o presionar el S.A.Derecho).\r\n" +
				"\r\n" +
				"Haz la prueba alejando o acercando la cámara para luego reiniciar la distancia de esta.\r\n " +
				"\r\n" +
				"\r\n" +
				"Reinicia la distancia de la cámara para continuar.\r\n");
			break;

		case 7:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Vamos a ver el inventario. Eres pobre y no llevas nada encima." +
				"Oprime Abajo en el Directional Pad (nº4 en teclado) una vez para abrirlo y otra vez para cerrarlo." +
				"El inventario abierto equivale a poner pausa al juego; el tiempo se detendrá." +
				"\r\n" +
				"He colocado unas espadas cerca del punto de inicio, ve a buscarlas.\r\n " +
				"Tan solo tocarlas basta para recogerlas y que aparezcan en el inventario.\r\n " +
				"\r\n" +
				"Recoge las espadas.\r\n");
			break;

		case 8:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Bien bien, ahora abre el inventario oprimiendo Abajo en el Directional Pad (nº4 en teclado). \r\n " +
				"Verás que ahora tienes armas guardadas. Para equiparte con una, tan solo oprime el botón con su\r\n " +
				"nombre. Desapracerá de tu inventario pero la llevaras enfundada y a mano para una pelea.\r\n" +
				"\r\n" +
				"\r\n" +
				"Elige una o varias espadas y a continuación cierra el inventario.\r\n");
			break;

		case 9:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Ahora desenvaina tu espada oprimiendo Arriba en el Directional Pad (nº 2 en teclado). \r\n " +
				"Podrás ir cambiando de arma dependiendo de cuales hayas seleccionado previamente en el inventario.\r\n " +
				"Cada arma proporciona diferentes combinaciones de habilidades a la hora de pelear.\r\n" +
				"\r\n" +
				"\r\n" +
				"Desenfunda una espada.\r\n");
			break;

		case 10:
			GUI.Box (new Rect (10, 60, 240, 300),
				"Haré aparecer un enemigo en la escena, pero no te preocupes, no va a atacar. " +
				"Nos será útil para aprender la dinámica de combate. " +
				"Como ya dije, cada arma proporciona diferentes combinaciones de habilidades a la hora de pelear.\r\n" +
				"\r\n" +
				"Oprime y mantén presionado el botón de 'Combo Mode' (R1 o Ctrl en teclado, creo jaja). " +
				"En el modo de combate tu personaje apuntara automáticamente al algun enemigo cercano, y solo\r\n " +
				"en modo de combate pordrás hacer trucos con tu espada.\r\n" +
				"\r\n" +
				"Oprime 'Usar' (Por defecto F o Círculo) para continuar\r\n");
			break;

		case 11:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Spin Attack: Izq + Der + Izq + Attack (Triángulo o R)\r\n" +
				"Forward Attack: Adelante + Atrás + Adelante + Attack (Triángulo o R)\r\n" +
				"Air Attack: Salto + Attack (Triángulo o R)\r\n" +
				"\r\n" +
				"Puedes ver mas movimientos en el menu de pausa.\r\n" +
				"\r\n" +
				"Como ya dije, cada arma proporciona diferentes combinaciones de habilidades a la hora de pelear.\r\n" +
				"No todos los trucos están disponibles en cada arma. Prueba con diferentes.\r\n" +
				"\r\n" +
				"Derrota a tu enemigo para continuar\r\n");
			break;

		case 12:
			GUI.Box (new Rect (10, 60, 240, 300),
				"Muy bien! Ese ya no molestará! (ni que lo hubiera estado haciendo)\r\n" +
				"\r\n" +
				"Puede hacer saltos de combate en modo de combate.\r\n" +
				"Creo que vendría a llamarse algo así como Leap o Bounce.\r\n" +
				"Te permitirá esquivar con agilidad o acercarte rapidamente a un oponente. Su potencia depende de tu velocidad de movimiento y un poco de la habilidad de salto\r\n" +
				"\r\n" +
				"En modo de Combate (R1 o ctrl):.\r\n" +
				"Stick A. Izquierdo (W,A,S,D) en alguna dirección + Salto\r\n" +
				"\r\n" +
				"Haz tres saltos de combate para continuar\r\n");
			break;

		case 13:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Bueno, para ir terminando vamos a pasar a las armas de fuego.\r\n" +
				"\r\n" +
				"Se recogen igual que las espadas, y se equipan de la misma manera.\r\n" +
				"te he dado un arma de fuego. Está en tu inventario.\r\n" +
				"Equipala y seleccionala como arma activa.\r\n" +
				"\r\n" +
				"\r\n" +
				"Equipa tu arma de fuego para continuar (Pad direccional hacia la izquierda, nº1 en teclado).\r\n");
			break;

		case 14:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Disparas con el botón de disparo (dah)\r\n" +
				"Por defecto es Cuadrado o Click izquierdo del Mouse.\r\n" +
				"En modo de combate el autoaim disparará a los enemigos cercanos.\r\n" +
				"Proximamente vere de agregar una opcion para activar y desactivar el autoaim.\r\n" +
				"\r\n" +
				"Con el modo apuntar puedes apuntar con mayor precision (L1 o Click derecho del Mouse).\r\n" +
				"\r\n" +
				"Dispara al enemigo que está sobre la caja más grande para continuar.\r\n");
			break;

		case 15:
			GUI.Box (new Rect (10, 70, 200, 275),
				"Una verdadera masacre.\r\n" +
				"\r\n" +
				"Bueno, creo que eso es todo. Ah, todavía está en fase experimental,\r\n" +
				"Pero puedes grabar la partida con F5, y recargarla con F6 (o era F9?).\r\n" +
				"\r\n" +
				"Escape o Start son pausa, con un botón para volver al menú principal.\r\n" +
				"\r\n" +
				"Y bueno, eso. Que te diviertas!\r\n" + 
				"Oprime 'Usar' (Por defecto F o Círculo) para continuar\r\n");
			
			break;
		}
	}
}
