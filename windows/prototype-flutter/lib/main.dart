// Prototype V0 Ibada — fenêtre opérateur + fenêtre projection plein écran.
// Objectif (cahier, section 3) : texte net avec ombre et contour, vidéo en boucle,
// second écran plein écran, démarrage rapide.

import 'dart:async';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:video_player/video_player.dart';
import 'package:window_manager/window_manager.dart';
import 'package:screen_retriever/screen_retriever.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await windowManager.ensureInitialized();

  const operatorOptions = WindowOptions(
    size: Size(1000, 640),
    center: true,
    title: 'Ibada — Opérateur (prototype V0)',
    titleBarStyle: TitleBarStyle.normal,
  );
  await windowManager.waitUntilReadyToShow(operatorOptions, () async {
    await windowManager.show();
    await windowManager.focus();
  });

  runApp(const IbadaPrototypeApp());
}

class IbadaPrototypeApp extends StatelessWidget {
  const IbadaPrototypeApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Ibada V0',
      debugShowCheckedModeBanner: false,
      theme: ThemeData.dark(useMaterial3: true),
      home: const OperatorWindow(),
    );
  }
}

/// Fenêtre opérateur : aperçu + boutons de test.
class OperatorWindow extends StatefulWidget {
  const OperatorWindow({super.key});
  @override
  State<OperatorWindow> createState() => _OperatorWindowState();
}

class _OperatorWindowState extends State<OperatorWindow> {
  int _currentSlide = 0;

  static const _slides = <String>[
    'Bienvenue à Ibada\nPrototype V0',
    'Texte net\navec ombre et contour',
    'Vidéo en boucle\nen arrière-plan',
    'Second écran\nplein écran',
  ];

  void _next() => setState(() => _currentSlide = (_currentSlide + 1) % _slides.length);
  void _prev() => setState(() => _currentSlide = (_currentSlide - 1 + _slides.length) % _slides.length);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Ibada — Opérateur (V0)')),
      body: Column(
        children: [
          Expanded(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: SlideView(text: _slides[_currentSlide]),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(12),
            child: Wrap(
              spacing: 12,
              children: [
                ElevatedButton.icon(
                  onPressed: _prev,
                  icon: const Icon(Icons.arrow_back),
                  label: const Text('Précédent'),
                ),
                ElevatedButton.icon(
                  onPressed: _next,
                  icon: const Icon(Icons.arrow_forward),
                  label: const Text('Suivant'),
                ),
                OutlinedButton.icon(
                  onPressed: _openProjectionWindow,
                  icon: const Icon(Icons.tv),
                  label: const Text('Ouvrir la projection'),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _openProjectionWindow() async {
    final displays = await screenRetriever.getAllDisplays();
    final target = displays.length > 1 ? displays[1] : displays.first;
    await windowManager.setFullScreen(true);
    // Note : multi_window n'est pas stable sur Windows pour l'instant.
    // En attendant, on bascule la fenêtre opérateur en plein écran sur
    // l'écran secondaire — suffisant pour valider la fluidité vidéo en V0.
    await windowManager.setPosition(Offset(target.visiblePosition!.dx, target.visiblePosition!.dy));
    setState(() {});
  }
}

/// Rendu d'une diapositive : fond vidéo en boucle + texte ombré/contour.
class SlideView extends StatefulWidget {
  final String text;
  const SlideView({super.key, required this.text});

  @override
  State<SlideView> createState() => _SlideViewState();
}

class _SlideViewState extends State<SlideView> {
  VideoPlayerController? _video;

  @override
  void initState() {
    super.initState();
    _initVideo();
  }

  Future<void> _initVideo() async {
    final controller = VideoPlayerController.asset('assets/test-video.mp4');
    try {
      await controller.initialize();
      await controller.setLooping(true);
      await controller.setVolume(0);
      await controller.play();
      if (mounted) setState(() => _video = controller);
    } catch (_) {
      // Pas de vidéo : on garde le fond sombre. V0 doit rester utilisable.
    }
  }

  @override
  void dispose() {
    _video?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AspectRatio(
      aspectRatio: 16 / 9,
      child: Container(
        color: Colors.black,
        child: Stack(
          fit: StackFit.expand,
          children: [
            if (_video != null && _video!.value.isInitialized)
              FittedBox(
                fit: BoxFit.cover,
                child: SizedBox(
                  width: _video!.value.size.width,
                  height: _video!.value.size.height,
                  child: VideoPlayer(_video!),
                ),
              ),
            Center(
              child: Padding(
                padding: const EdgeInsets.all(32),
                child: Text(
                  widget.text,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    fontSize: 64,
                    fontWeight: FontWeight.bold,
                    color: Colors.white,
                    height: 1.2,
                    shadows: [
                      Shadow(offset: Offset(3, 3), blurRadius: 6, color: Colors.black),
                      Shadow(offset: Offset(-2, -2), blurRadius: 4, color: Colors.black),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}