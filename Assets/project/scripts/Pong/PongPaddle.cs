using UnityEngine;

namespace Pong
{

    public class PongPaddle : MonoBehaviour
    {
        public PongControls controls;
     
        public enum Direction
        {
            Up,
            Down

        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(controls.upKey))
                Move(Direction.Up);            
            
            if (Input.GetKey(controls.downKey))
                Move(Direction.Down);   
        }

        void Move(Direction direction)
        {
            {
                float moveDistance = controls.paddleSpeed * Time.deltaTime;
                moveDistance *= direction == Direction.Up ? 1 : -1;

                Vector3 moveVector = new Vector3(0, moveDistance, 0);

                if(transform.position.y + moveDistance > controls.maxY)
                {
                    transform.position =
                        new Vector3(
                            transform.position.x,
                            controls.maxY,
                            transform.position.z
                            );
                }
                else if (transform.position.y + moveDistance < controls.minY)
                {
                    transform.position =
                       new Vector3(
                           transform.position.x,
                           controls.minY,
                           transform.position.z
                           );

                }
                else
                {
                    transform.Translate(moveVector);
                }
                    

                Debug.Log(direction.ToString() + "gedrückt");


           

            }

        }
    }
}
