# Marble Racing Game — TODO

## Current Status
Phase 1 COMPLETE. Phase 2 & 3 mostly complete. Core racing loop works with visual polish.

---

## Phase 1: Core Racing (COMPLETE)

### Done
- [x] Track generation with smooth curves (80 units, 3 S-curves)
- [x] 8 marbles with physics-based racing
- [x] Betting panel with colored marble selection
- [x] Race HUD showing positions with color indicators
- [x] Camera follows marble group, ignores fallen marbles
- [x] Start gate holds marbles before countdown
- [x] Finish line detection with bucket catch area
- [x] 4 obstacles (cylinder bumpers + spinning bar + boost pad)
- [x] Randomized starting positions each race
- [x] Balanced marble stats (no marble dominates)
- [x] Coin economy (wallet, betting, payouts)
- [x] Results panel with winner display
- [x] Sound effects (collision, bet placed, countdown)

---

## Phase 2: Full Game Loop (COMPLETE)

- [x] Multiple track types (Zigzag, Funnel, Spiral, Downhill)
- [x] Random track rotation between races
- [x] Daily bonus coins (200 coins/day)
- [x] Bailout system (100 free coins if balance hits 0, once per hour)
- [x] Persistent coin balance (PlayerPrefs)
- [x] Race history / stats screen (RaceStatsManager + StatsPanel)
- [x] More obstacle types (boost pads, cylinder bumpers, spinners)
- [ ] Marble unlock system (start with 4, unlock more)

---

## Phase 3: Visual Polish (COMPLETE)

- [x] Marble trail effects (colored trail behind each marble)
- [x] Better track materials (metallic floor, reflective walls)
- [x] Particle effects on obstacle hits (colored burst per hazard type)
- [x] Marble dust/spark particles (MarbleParticles)
- [x] Post-processing (bloom, vignette, color grading, film grain)
- [x] Emissive glow on obstacles (red bumpers, orange spinner, green boost)
- [x] Winner celebration (confetti, slow-mo, camera orbit)
- [x] Higher quality rendering (4x MSAA, HDR, VeryHigh shadows)
- [ ] Crowd/ambient sound during race
- [ ] UI animations (panel slides, button feedback)
- [ ] Skybox / environment art

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
