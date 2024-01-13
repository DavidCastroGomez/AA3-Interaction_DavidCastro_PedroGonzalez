# AA2-Inverse_Kinematics_DavidCastro_PedroGonzalez

AUTHORS

Group: D

Name: David Castro Gómez

Email: david.castro@enti.cat

![276546966-c9632bb6-2ce8-40b8-ac5e-e56574061f43](https://github.com/DavidCastroGomez/AA3-Interaction_DavidCastro_PedroGonzalez/assets/99645935/50e63662-8361-4196-952d-7ac6cfbfd45a)

Name: Pedro González Navarro

Email: pedro.gonzalez@enti.cat

![Yo](https://github.com/DavidCastroGomez/AA1-ManualMovement_DavidCastro_PedroGonzalez/assets/99645935/151781d6-536e-4ca4-8afc-d8631f08d504)

MAGNUS FORMULA:

velocity += (acceleration + new Vector3(-magnus, 0, 0)) * Time.deltaTime;

position += velocity * Time.deltaTime;

EXERCISE IMPLEMENTATION LOCATION:

EXERCISE 1:
1.1: Ball and octopus collision detection in "MovingBall", search for the ball or not in "MyOctopusController".
1.2: We get rid of the horizontal/vertical input in the "MovingBall" and add it to the "Moving target" if is of type "USERTARGET", then calculate direction between the two in "MovingBall".
1.3: Input added in "IK_Scorpion".
1.4: We added a object in scene called "ResetScene" with a script with the same name. From there it calls to reset certain things.

EXERCISE 2:
Z/X inputs are in "IK_Scorpion". All logic is in "MovingBall" and there are new objects in scene (AngularVelocityText, targetBlue, targetGrey, arrow_appliedForce, arrow_gravity & arrow_magnus).

EXERCISE 3:
All new code is in "IK_Scorpion" and "MyScopionController".

EXERCISE 4:
All new code is in "IK_Scorpion" and "MyScopionController".

EXERCISE 5:
All new code is in "MyScorpionController".
