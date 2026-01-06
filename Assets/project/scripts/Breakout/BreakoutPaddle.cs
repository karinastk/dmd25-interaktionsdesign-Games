using UnityEngine;

namespace Breakout
{

    public class BreakoutPaddle : MonoBehaviour
    {
        public BreakoutControls controls;

        public enum Direction
        {
            Left,
            Right

        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(controls.leftKey))
                Move(Direction.Left);

            if (Input.GetKey(controls.rightKey))
                Move(Direction.Right);
        }

        void Move(Direction direction)
        {
            {
                float moveDistance = controls.paddleSpeed * Time.deltaTime;
                moveDistance *= direction == Direction.Right ? 1 : -1;

                Vector3 moveVector = new Vector3(moveDistance, 0, 0);

                if (transform.position.x + moveDistance > controls.maxX)
                {
                    transform.position =
                        new Vector3(                           
                            controls.maxX,
                            transform.position.y,
                            transform.position.z
                            );
                }
                else if (transform.position.x + moveDistance < controls.minX)
                {
                    transform.position =
                       new Vector3(
                           controls.minX,
                           transform.position.y,
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