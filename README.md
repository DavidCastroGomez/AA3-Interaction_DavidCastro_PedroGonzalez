# AA2-Inverse_Kinematics_DavidCastro_PedroGonzalez

Authors 

Group: D

Name: David Castro Gómez

Email: david.castro@enti.cat

![IMG_20231019_102656](https://github.com/DavidCastroGomez/AA1-ManualMovement_DavidCastro_PedroGonzalez/assets/99954770/c9632bb6-2ce8-40b8-ac5e-e56574061f43)

Name: Pedro González Navarro

Email: pedro.gonzalez@enti.cat

![Yo](https://github.com/DavidCastroGomez/AA1-ManualMovement_DavidCastro_PedroGonzalez/assets/99645935/151781d6-536e-4ca4-8afc-d8631f08d504)

Formula for the magnus:
   Vector3 newVelocity = acceleration * Time.deltaTime;
   velocity += newVelocity + new Vector3(-magnus, 0, 0);
   position += velocity * Time.deltaTime;