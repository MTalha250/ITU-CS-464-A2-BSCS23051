# CS464 Assignment 02 · Muhammad Talha · BSCS23051

- **Video:** [Run through all five levels (66 s, Google Drive)](https://drive.google.com/file/d/1GHMzvcvgV1dikPs0nA4QrrQRZfhpvxUB/view?usp=sharing)
- **Input:** `InputSystem.actions`: the project-wide Move and Jump actions (Active Input Handling: Input System).
- **Ground check:** contact normal: a contact counts as ground when `Vector3.Dot(normal, Vector3.up) > 0.5f`; the last ground contact is drawn with `Gizmos.DrawRay` in `OnDrawGizmosSelected` (green when grounded).
- **Camera:** `Vector3.SmoothDamp` in `LateUpdate`, with look-ahead from the player's flat velocity. One `Camera` prefab in every level finds the player with `FindAnyObjectByType<PlayerController>()`.
- **Juice:** a squash on landing: `Body` scales to y = 0.7 (x and z 1.2, same volume) and springs back through `Ease.OutBack` over 0.25 s, run by a coroutine. It starts on a landing after more than 0.1 s in the air.

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | One jump at 5 m/s carries 5.1 m, so the widest gap (3 m, Level01 and Level05) leaves 2 m spare, and the 1.5 m corridor in Level04 is still easy to steer. |
| Jump height | 1.8 m | Almost the capsule's own 2 m height. It clears every 0.3 m step with room to spare and gives the 3 m gaps a forgiving arc. |
| Time to apex | 0.61 s | Follows from the height: v₀ = √(2·9.81·1.8) = 5.94 m/s, and v₀ / g = 0.61 s. |
| Fall multiplier | 2.2× | The fall takes √(2·1.8 / (2.2·9.81)) = 0.41 s against 0.61 s up, so the jump lands in 1.01 s instead of 1.21 s and feels heavy, not floaty. |
| Coyote time | 0.1 s | At 5 m/s that is 0.5 m past the edge: half the capsule's width, enough for a late press at the 3 m gaps. |
| Jump buffer | 0.12 s | With 2.2× fall gravity the capsule covers its last 0.9 m in 0.12 s, so a press up to 0.9 m above the floor still jumps on landing. |
| Camera smoothing | smoothTime 0.3 s | SmoothDamp alone trails a 5 m/s run by about 1.5 m (≈ speed × smoothTime): close enough to read the jump, loose enough not to shake on landing. |
| Look-ahead | 0.35 s | 5 m/s × 0.35 s = 1.75 m ahead, which cancels the 1.5 m lag: the camera leads by about 0.3 m and shows the gap before the player reaches it. |

## Four answers

1. **The jump, in numbers.** With g = 9.81 and jumpHeight = 1.8 m, the launch speed is v₀ = √(2gh) = √35.3 = 5.94 m/s. On the 1 kg Rigidbody the impulse, or jumpForce, is m·v₀ = 5.94 N·s. It rises for v₀ / g = 0.61 s. Falling at 2.2 × 9.81 = 21.6 m/s², it comes down from 1.8 m in √(2·1.8 / 21.6) = 0.41 s, so it is in the air for 1.01 s. At 5 m/s that is 5.1 m of reach on flat ground, enough for the widest gap, 3 m in Level01 and Level05, with 2 m spare. Unity's 0.02 s steps make the real apex about 3% lower, roughly 1.74 m, which still clears every 0.3 m step.
2. **Update and FixedUpdate.** Update runs every frame. It reads Move and Jump, builds `move` from the camera's flattened forward and right, stamps the coyote and buffer timestamps, queues the jump when both windows are open, and starts the squash. FixedUpdate runs every 0.02 s. It sets `rb.linearVelocity` from `move`, applies a queued jump once, and adds fall gravity. Swap them and `WasPressedThisFrame` is read only on frames that happen to run a physics step: at 144 fps about two presses in three would be lost. The physics would also run at the frame rate, so the jump and fall would change with the machine.
3. **Vector3.Dot.** The ground check: `if (Vector3.Dot(p.normal, Vector3.up) > 0.5f)` in `OnCollisionStay`. Both vectors have length 1, so the dot is the cosine of the angle between the surface normal and straight up. Above 0.5 means the surface faces up within 60°, so a floor or the 30° ramp in Level02 counts as ground. A wall gives about 0 and a ceiling gives a negative value, so neither does, and the player can't jump off a wall in mid-air.
4. **The juice.** `Ease.OutBack` over 0.25 s, which is 15 frames at 60 fps. The squash starts at y = 0.7 and passes about 10% beyond the rest shape before it settles, so the landing reads as weight that springs back. At 0.25 s it is over before a buffered jump can fire again, so it never fights the next takeoff. It ends by setting the scale to exactly (1, 1, 1).

## Levels

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 | Run the corridor and the yard, then jump the 3 m gap to the goal. | Nothing: 5.1 m of reach clears 3 m with 2 m spare. |
| Level02 | Hop up five 0.3 m steps to a 1.5 m landing, then walk up the 30° ramp to the 3 m deck. | Nothing: each step is far below the 1.8 m jump, and the ramp counts as ground (its normal's dot with up is 0.87). |
| Level03 | Cross the 1.5 m bridge, with 0.25 m spare on each side of the capsule, or take the long route past the reward room. | Nothing: there is no jump, and camera-relative movement keeps W straight along the bridge. |
| Level04 | Steer the 1.5 m corridor round two turns, then hop three 0.3 m steps onto the 1.2 m dais. | Nothing: the steps are well within the jump. |
| Level05 | Hop the steps onto the 1.5 m deck, take the narrow bridge or the wide path, then jump the 3 m gap to the goal. | Nothing: the same 3 m gap as Level01, with 2 m spare. |
