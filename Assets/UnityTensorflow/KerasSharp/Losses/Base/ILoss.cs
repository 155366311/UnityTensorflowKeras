﻿
/// <summary>
///   Common interface for loss functions.
/// </summary>
/// 
public interface ILoss
{
    /// <summary>
    ///   Wires the given ground-truth and predictions through the desired loss.
    /// </summary>
    /// 
    /// <param name="expected">The ground-truth data that the model was supposed to approximate.</param>
    /// <param name="actual">The actual data predicted by the model.</param>
    /// 
    /// <returns>A scalar value representing how far the model's predictions were from the ground-truth.</returns>
    /// 
    Tensor Call(Tensor expected, Tensor actual, Tensor sample_weight = null, Tensor mask = null);
}