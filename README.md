# CS464 Assignment 02 · Muhammad Talha · BSCS23051

- **Video:** [Run through all five levels (66 s, Google Drive)](https://drive.google.com/file/d/1GHMzvcvgV1dikPs0nA4QrrQRZfhpvxUB/view?usp=sharing)
- **Input:** InputSystem.actions (the project-wide Move and Jump actions). Active Input Handling is left on Input System.
- **Ground check:** contact normal. A contact counts as ground if `Vector3.Dot(normal, Vector3.up) > 0.5f`. With the player selected, the gizmo draws the last ground contact as a ray: green when grounded, red when not.
- **Camera:** SmoothDamp in LateUpdate, with look-ahead. There's one Camera prefab in every level, and it finds the player itself with `FindAnyObjectByType<PlayerController>()`.
- **Juice:** squash on landing. The Body goes down to 0.7 in y (and 1.2 in x and z so the volume stays the same), then springs back with Ease.OutBack over 0.25 s. A coroutine runs it, and it only starts if I was in the air for more than 0.1 s.

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 5 m/s | One jump at this speed goes about 5.1 m, so my widest gap (3 m in Level01 and Level05) leaves 2 m spare. It's also still easy to steer in Level04's 1.5 m corridor. |
| Jump height | 1.8 m | Almost the capsule's height (2 m). It clears the 0.3 m steps easily and gives a nice arc over the 3 m gaps. |
| Time to apex | 0.61 s | Comes from the height. v0 = √(2 × 9.81 × 1.8) = 5.94 m/s, and 5.94 / 9.81 = 0.61 s. |
| Fall multiplier | 2.2× | The fall takes 0.41 s instead of 0.61 s, so a jump lasts 1.01 s instead of 1.21 s. It feels heavier and less floaty. |
| Coyote time | 0.1 s | At 5 m/s that's 0.5 m past the edge, half the capsule's width. That's enough to forgive a late press at the gaps. |
| Jump buffer | 0.12 s | With 2.2× gravity the capsule falls its last 0.9 m in 0.12 s, so pressing jump up to 0.9 m above the floor still works. |
| Camera smoothing | smoothTime 0.3 s | On its own, SmoothDamp trails about 1.5 m behind at 5 m/s (5 × 0.3). That's close enough to see the jump without the camera shaking on landing. |
| Look-ahead | 0.35 s | 5 × 0.35 = 1.75 m ahead, which more than cancels the 1.5 m lag. The camera ends up about 0.3 m ahead, so I can see the gap coming. |

## Four answers

1. **The jump, in numbers.** With g = 9.81 and jumpHeight = 1.8 m, the launch speed is √(2gh) = 5.94 m/s. My Rigidbody is 1 kg, so the impulse (jumpForce) is m × v0 = 5.94 N·s. Going up takes 5.94 / 9.81 = 0.61 s. Coming down, gravity is 2.2 × 9.81 = 21.6 m/s², so falling 1.8 m takes √(2 × 1.8 / 21.6) = 0.41 s. That's 1.01 s in the air, and at 5 m/s that's about 5.1 m. My widest gap is 3 m (Level01 and Level05), so it clears with 2 m to spare. In Unity the real apex is about 3% lower (around 1.74 m) because of the 0.02 s physics steps, but that still clears every step.

2. **Update and FixedUpdate.** Update runs every frame. It reads Move and Jump, builds the move direction from the camera's forward and right, and updates the coyote and buffer timestamps. It queues the jump when both windows are open, and starts the squash on landing. FixedUpdate runs every 0.02 s and does the physics: it sets `rb.linearVelocity` from the move direction, applies the queued jump once, and adds the extra fall gravity. If I swapped them, `WasPressedThisFrame` would only get checked on frames that also run a physics step, so at 144 fps I'd miss about two jump presses out of three. The physics would also depend on the frame rate, so the jump would change from one PC to another.

3. **Vector3.Dot.** I use it in the ground check: `if (Vector3.Dot(p.normal, Vector3.up) > 0.5f)` in OnCollisionStay. Both vectors have length 1, so the dot product is the cosine of the angle between the surface normal and straight up. Anything over 0.5 faces up within 60°, so floors and the 30° ramp in Level02 count as ground. A wall gives about 0 and a ceiling gives a negative value, so neither counts, and you can't jump off a wall in mid-air.

4. **The juice.** The curve is Ease.OutBack and it lasts 0.25 s, which is 15 frames at 60 fps. The capsule starts squashed (y = 0.7), goes about 10% past normal, then settles, so it looks like it has some weight and bounces back. I kept it short so it's finished before I'd normally jump again. At the end it sets the scale back to exactly (1, 1, 1).

## Levels

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 | Run through the corridor and the yard, then jump the 3 m gap to the goal. | Nothing. The jump goes about 5.1 m, so 3 m is fine. |
| Level02 | Hop up five 0.3 m steps to the 1.5 m landing, then walk up the 30° ramp to the 3 m deck. | Nothing. The steps are way under the 1.8 m jump, and the ramp counts as ground. |
| Level03 | Cross the 1.5 m bridge (0.25 m spare on each side), or take the long way past the reward room. | Nothing. There's no jump here, and camera-relative movement keeps W straight along the bridge. |
| Level04 | Get through the 1.5 m corridor and its two turns, then hop up three 0.3 m steps onto the 1.2 m dais. | Nothing. The steps are easy with this jump. |
| Level05 | Hop up the steps to the 1.5 m deck, take either the narrow bridge or the wide path, then jump the 3 m gap. | Nothing. It's the same 3 m gap as Level01. |
