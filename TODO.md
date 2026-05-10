# Marble Racing Game — TODO

## Current Status
Phase 1 MVP is partially complete. Core racing loop works (betting > countdown > race > results).

---

## Phase 1: Core Racing (IN PROGRESS)

### Done
- [x] Track generation with smooth curves (80 units, 3 S-curves)
- [x] 8 marbles with physics-based racing
- [x] Betting panel with colored marble selection
- [x] Race HUD showing positions with color indicators
- [x] Camera follows marble group, ignores fallen marbles
- [x] Start gate holds marbles before countdown
- [x] Finish line detection with bucket catch area
- [x] 3 obstacles (cylinder bumpers + spinning bar)
- [x] Randomized starting positions each race
- [x] Balanced marble stats (no marble dominates)
- [x] Coin economy (wallet, betting, payouts)
- [x] Results panel with winner display
- [x] Sound effects (collision, bet placed, countdown)

### TODO
- [ ] Fix: Frost marble stats — verify balance after asset regeneration
- [ ] Fix: Ensure all obstacles render as cylinders (not cubes)
- [ ] Polish: Add race replay button that works
- [ ] Polish: Smooth countdown animation (3, 2, 1, GO!)
- [ ] Polish: Winner celebration effect (camera orbit, slow-mo)
- [ ] Polish: Better lighting on obstacles (emissive glow)
- [ ] UX: Show selected marble highlight on betting panel
- [ ] UX: Show payout amount on results screen
- [ ] UX: Auto-restart after results (or tap to continue)

---

## Phase 2: Full Game Loop

- [ ] Multiple track types (zigzag, funnel, spiral) — currently only curved
- [ ] Track selection or random rotation between races
- [ ] Daily bonus coins (200 coins/day)
- [ ] Bailout system (100 free coins if balance hits 0, once per hour)
- [ ] Persistent coin balance (save to PlayerPrefs or file)
- [ ] Race history / stats screen
- [ ] More obstacle types (boost pads, jump ramps, hammers)
- [ ] Marble unlock system (start with 4, unlock more)

---

## Phase 3: Visual Polish

- [ ] Marble trail effects (colored trail behind each marble)
- [ ] Better track materials (textured, not flat color)
- [ ] Particle effects on obstacle hits
- [ ] Crowd/ambient sound during race
- [ ] UI animations (panel slides, button feedback)
- [ ] Skybox / environment art
- [ ] Post-processing (bloom, vignette, color grading)

---

## Phase 4: Mobile Release

- [ ] Touch input optimization
- [ ] UI scaling for different screen sizes
- [ ] Performance profiling (target 60fps on mobile)
- [ ] Build for Android APK
- [ ] Build for iOS TestFlight
- [ ] App icon and splash screen
- [ ] Monetization: ads between races (optional rewarded ads for bonus coins)
- [ ] Monetization: premium marble skins (IAP)

---

## Phase 5: Multiplayer & Social

- [ ] Online multiplayer betting (multiple real players in same race)
- [ ] Lobby system (join/create rooms)
- [ ] Leaderboard (most coins earned)
- [ ] Share race results
- [ ] Friend challenges
- [ ] Backend server (Supabase or similar)
- [ ] User accounts and authentication

---

## Phase 6: Content & Live Ops

- [ ] 10+ track designs
- [ ] Seasonal events (special tracks, limited marble skins)
- [ ] Tournament mode (bracket-style elimination)
- [ ] Spectator mode
- [ ] Analytics (track which marbles win most, player retention)
- [ ] Push notifications for daily rewards
