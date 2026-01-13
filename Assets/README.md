# Hand Interaction System - VR Training Demo

A hand interaction system for Unity XR showcasing runtime pose blending, procedural finger animation, and flexible bone resolver architecture. Developed as a portfolio demonstration of VR hand animation techniques.

![Demo GIF ]

---

## 🎯 Core Features

### ✨ Runtime Hand Animation
- **Pose-based blending system** - Smooth transitions between pre-authored hand poses
- **ScriptableObject data pipeline** - Artist-friendly workflow for pose authoring
- **Procedural finger curl** - Real-time finger animation driven by controller input (0-1 curl values)
- **Custom editor tooling** - In-editor pose capture and preview system

### 🔧 Technical Highlights
- **Regex-based bone name normalization** - Automatic normalization of bone hierarchies from different hand models
- **Cross-model compatibility** - Works with custom rigs and XR Hands canonical models
- **Hand mirroring system** - Single left-hand pose automatically mirrors to right hand
- **Event-driven architecture** - Clean separation between animation, interaction, and input layers

### 🎮 VR Interaction Integration
- **XR Interaction Toolkit integration** - Seamless integration with Unity's XR framework
- **Multi-hand grab support** - Simultaneous two-handed interactions
- **Dynamic hand detachment** - Hands detach from controllers and follow grabbed objects
- **Attach point system** - Precise hand placement on interactable objects

---

## 📋 Designed For

**VR training applications requiring realistic hand interactions:**

- Equipment operation simulation (valves, switches, control panels)
- Manual task training (assembly, inspection, tool usage)
- Safety procedure practice (equipment handling, emergency response)

**Current demo:** Basic grab interactions with valve/object manipulation  
**Extensible to:** Any VR scenario requiring dynamic hand poses

*This system provides the foundation — specific training scenarios require domain expertise integration*

---

## 🏗️ Architecture Overview
```
HandAnimator (Core Controller)
├── IBoneResolver (Bone Naming Strategy)
│   ├── ExactNameBoneResolver
│   └── NormalizedNameBoneResolver
│
├── HandPoseSO (ScriptableObject Data)
│   └── JointData[] (position + rotation per bone)
│
└── Core Capabilities:
    ├── Instant pose switching (AnimateInstantly)
    ├── Smooth pose blending (BeginNewPoses + coroutine lerp)
    ├── Procedural finger curl (ApplyFingerCurl)
    ├── Hand detachment from controller
    └── Object tracking (MoveHandToTarget + LateUpdate)
```

---

## 🔑 Key Technical Solutions

### 1. Canonical Bone Naming System
Automatically normalizes bone names from different hand models using regex patterns:

```
Input variations:
- "L_Index_01_Jnt"
- "Left_Index_Proximal"  
- "LeftIndexProximal"
- "Index1"

↓ Normalized to ↓

"index_proximal"
```

**Benefits:**
- Single pose works across multiple hand models
- No manual remapping required
- Supports custom rigs and standard models (XR Hands, MetaHuman, etc.)

**Implementation:** Uses regex-based parsing to strip prefixes/suffixes and convert digit notation to semantic names (e.g., "1" → "proximal").

### 2. Procedural Finger Curl
Runtime finger animation without pre-authored poses:
```csharp
// Define open and fist reference poses
public HandPoseSO openPose;
public HandPoseSO fistPose;

// Each finger has independent curl control (0 = open, 1 = fist)
fingerChains[i].curlValue = triggerInput; // From XR controller

// Interpolate between reference poses per-bone
ApplyFingerCurl(fingerChain);
```

**Use case:** Dynamic hand reactions to controller input (trigger squeeze = gradual fist)

### 3. Hand Mirroring Algorithm
Left-hand poses automatically flip to right hand:

```csharp
public JointTransformData MirrorJoint(Vector3 localPos, Quaternion localRot, string boneName)
{
    Vector3 mirroredPos = new Vector3(-localPos.x, localPos.y, localPos.z);
    Vector3 euler = localRot.eulerAngles;
    Quaternion mirroredRot = Quaternion.Euler(euler.x, -euler.y, -euler.z);
    return new JointTransformData { pos = mirroredPos, rot = mirroredRot };
}
```

**Benefit:** Artists author half the poses, system guarantees symmetry

---

## 📦 Project Structure
```
_Project/
├── HandAnimator.cs              # Core animation controller
├── HandPoseSO.cs                # Pose data ScriptableObject
├── BaseHandPose.cs              # Base interaction component
├── HandPoseOnFixedGrab.cs       # Fixed attachment (e.g. valve wheel)
│
├── BoneResolvers/
│   ├── IBoneResolver.cs         # Resolver interface
│   ├── ExactNameBoneResolver.cs
│   └── NormalizedNameBoneResolver.cs
│
├── Editor/
│   └── HandAnimatorEditor.cs    # Custom inspector with pose tools
│
└── Data/
    └── Poses/                   # HandPoseSO assets
        ├── HandOpen.asset       # Relaxed hand
        ├── HandFist.asset       # Fully closed fist
        └── ValveGrab_Left.asset # Example grab pose
```

---

## 🚀 Quick Start

### Setup (Inspector-based)

1. **Attach HandAnimator component** to your hand visual GameObject
2. **Assign Root Bone** - Drag the hand skeleton root transform
3. **Set Default Pose and close pose(fist) ** - Select a HandPoseSO asset, (e.g., HandPose_open.asset and HandPose_fist)
4. **Choose Bone Resolver** - Select "Normalized" type for cross-model support
5. **Setup Finger Chains** - Click "Setup Finger Chains" button in inspector for scan and setup bones
6. **Add BaseHandPose component to grabbable object** - choose pose gor grabbing (e.g HandPose_small_grab.asset, HandPosr_wheel_grab.asset.. etc)

**No scripting required** - All configuration via Unity Inspector

---

### Create Poses in Editor
1. Select hand model with HandAnimator in hierarchy
2. Click "Setup Finger Chains" button
2. Manually pose bones in scene view using fingers curls
3. Click "Save Current Pose" button
4. Pose saved as reusable ScriptableObject asset

---

### Apply at Runtime (Optional)

For programmatic control:

```csharp
// Instant pose change
handAnimator.AnimateInstantly(grabPose);

// Smooth blending
handAnimator.BeginNewPoses(primaryPose: grabPose, animPose: null);

// Procedural curl
fingerChain.curlValue = 0.8f; // 80% fist
handAnimator.ApplyFingerCurl(fingerChain);
```

---

## 🎨 Editor Features

### Pose Authoring Workflow
- **Visual pose editing** - Manipulate bones directly in scene view
- **One-click pose capture** - Saves all bone transforms to ScriptableObject
- **Instant preview** - Test poses without entering play mode
- **Finger curl sliders** - Real-time procedural animation preview

### Bone Resolver Configuration
- **Exact matching** - Direct name comparison (fast, strict)
- **Normalized matching** - Regex-based canonicalization (flexible, cross-model)

---

## 🔬 Technical Specifications

| Feature | Implementation                                           |
|---------|----------------------------------------------------------|
| **Framework** | Unity 2022.3 LTS, XR Interaction Toolkit 2.6.5+          |
| **VR Platform** | OpenXR (Quest, PCVR, SteamVR compatible)                 |
| **Render Pipeline** | Universal Render Pipeline (URP)                          |
| **Animation Method** | Transform hierarchy manipulation (no Animator component) |

---

## 📦 Project Setup

### Dependencies
- Unity 2022.3 LTS or later
- XR Interaction Toolkit 2.6.5+
- Universal Render Pipeline (URP)

**Note:** XRIT sample assets included for input bindings and XR Origin rig setup.

---

## ⚡ Performance Characteristics

- **Transform-based animation** - No Animator overhead, direct bone manipulation
- **Cached lookups** - Dictionary-based bone resolution (O(1) access)
- **Minimal allocations** - Coroutine reuse, lookup tables initialized once
- **Smooth hand tracking** - LateUpdate timing for stable object following

---

## 🛠️ Advanced Configuration

### Custom Bone Resolver
Implement `IBoneResolver` for custom naming conventions:
```csharp
public class MyCustomResolver : IBoneResolver
{
    public void Initialize(IReadOnlyDictionary<string, Transform> boneCache) { }
    
    public Transform Resolve(string jointName)
    {
        // Custom matching logic
        return matchedBone;
    }
    
    public string GetCanonicalName(string rawName)
    {
        // Normalization logic
        return canonicalName;
    }
}
```

### Extending Hand Interactions
Create custom grabbable behaviors by inheriting `BaseHandPose`:
```csharp
public class MyInteractable : BaseHandPose
{
    protected override void OnHandGrabbed(HandAnimator hand)
    {
        base.OnHandGrabbed(hand);
        // Custom logic: haptics, audio, effects
    }
}
```

---

## 🔮 Future Enhancements

### Planned Features
- [ ] **XR Hand Tracking integration** - Controller-free hand tracking (Quest 3)
- [ ] **Curl-driven animation** - Automatic pose generation from controller input
- [ ] **IK constraints** - Physics-based finger collision avoidance
- [ ] **Networked hand sync** - Multiplayer hand animation replication
- [ ] **Animation curve support** - Non-linear pose blending

### Extensibility
System designed with provider pattern for future animation sources:
- `PoseBasedProvider` (current implementation)
- `CurlBasedProvider` (planned - procedural from input)
- `HandTrackingProvider` (planned - XR Hands integration)

---

## ⚠️ Project Status

This is a **working development repository** demonstrating production-ready hand animation techniques. Some optimizations and features are marked as TODO in code for future refinement.

**Current state:** Fully functional for VR training applications  
**Code quality:** Production-ready architecture with documented improvement areas

For commercial integration or custom development, contact: aleksey.zernovv@gmail.com

---

## 📝 Code Quality

✅ **Interface-based design** - Swappable bone resolver strategies (IBoneResolver)  
✅ **Event-driven architecture** - XR Interaction Toolkit integration  
✅ **Minimal runtime allocations** - Coroutine reuse, object pooling ready  
✅ **Editor tooling** - Custom inspectors, one-click workflows  
✅ **Cross-platform** - OpenXR standard, device-agnostic

---

## 🎓 Techniques Demonstrated

- **ScriptableObject architecture** - Data-driven pose authoring
- **Custom Editor extensions** - Artist-friendly Unity tools
- **Strategy Pattern** - Swappable bone resolver implementations (IBoneResolver)
- **Coroutine-based animation** - Smooth blending without Animator overhead
- **Regex text processing** - Bone name normalization across different rigs

---

## 📄 License

This is a portfolio demonstration project.

The code is provided for viewing and educational reference only.

**Permissions:**
- View the source code
- Use code snippets for learning purposes with attribution

**Restrictions:**
- No commercial use
- No redistribution
- No derivative works without written permission

For licensing inquiries, contact: aleksey.zernovv@gmail.com

---

## 👤 Contact

**Developer:** Aleksey Zernov  
**Email:** aleksey.zernovv@gmail.com  
**LinkedIn:** [linkedin.com/in/aleksey-zernov-86145b191](https://www.linkedin.com/in/aleksey-zernov-86145b191)

**Specialization:** VR/XR interaction systems, procedural animation, Unity editor tooling

Interested in VR training applications, hand tracking systems, and technical art pipelines.

---

## 📸 Screenshots

[ ]
1. Custom editor with finger curl sliders
2. Hand grabbing valve wheel (both hands)
3. Pose authoring workflow
4. Inspector showing bone resolver settings
5. Runtime hand animation in VR scene

---

## 🎬 Demo Video

[Embedded YouTube video - 60-90 seconds showing:]
- Pose authoring in editor
- Runtime hand grab with animation
- Two-handed valve operation
- Procedural finger curl demo

---

**⭐ If you find this relevant for your VR training project, let's connect!**