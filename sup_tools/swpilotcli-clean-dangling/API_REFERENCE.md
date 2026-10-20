# API Reference

- `IAnnotation.IsDangling` determines whether an annotation is unattached.
- `IAnnotation.Select3` selects one candidate annotation.
- `IModelDocExtension.DeleteSelection2(0)` removes the selected annotation without a confirmation dialog.
- `swAnnotationType_e` filters DisplayDimension, CenterLine, CenterMarkSym, and Note annotations.
- `INote.GetBalloonStyle` distinguishes balloons from ordinary Notes.

The tool uses document traversal through `DrawingDoc.GetViews` and `IView.GetFirstAnnotation3` / `IAnnotation.GetNext3`.
