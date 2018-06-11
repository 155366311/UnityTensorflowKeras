﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu()]
public class RLNetworkSimpleAC : RLNetworkAC
{

    public int actorNNHidden = 1;
    public int actorNNWidth = 128;
    public int criticNNHidden = 1;
    public int criticNNWidth = 128;
    public float hiddenWeightsInitialScale = 1;
    public float outputWeightsInitialScale = 0.01f;

    protected List<Tensor> weights;


    public override void BuildNetwork(Tensor inVectorstate, List<Tensor> inVisualState, Tensor inMemery, Tensor inPrevAction, int outActionSize,
        out Tensor outAction, out Tensor outValue)
    {

        Debug.Assert(inMemery == null, "Currently recurrent input is not supported by RLNetworkSimpleAC");
        Debug.Assert(inPrevAction == null, "Currently previous action input is not supported by RLNetworkSimpleAC");
        Debug.Assert(!(inVectorstate == null && inVisualState == null), "Network need at least one vector observation or visual observation");

        weights = new List<Tensor>();

        //visual encoders
        Tensor encodedVisualActor = null;
        Tensor encodedVisualCritic = null;
        if (inVisualState != null)
        {
            List<Tensor> visualEncodedActor = new List<Tensor>();
            List<Tensor> visualEncodedCritic = new List<Tensor>();
            foreach (var v in inVisualState)
            {
                var ha = CreateVisualEncoder(v, actorNNWidth, actorNNHidden, "ActorVisualEncoder");
                var hc = CreateVisualEncoder(v, criticNNWidth, criticNNHidden, "CriticVisualEncoder");
                visualEncodedActor.Add(ha);
                visualEncodedCritic.Add(hc);
            }
            encodedVisualActor = Current.K.concat(visualEncodedActor, 1);
            encodedVisualCritic = Current.K.concat(visualEncodedCritic, 1);
        }

        //vector states encode
        Tensor encodedVectorStateActor = null;
        Tensor encodedVectorStateCritic = null;
        if (inVectorstate != null)
        {
            encodedVectorStateActor = CreateContinuousStateEncoder(inVectorstate, actorNNWidth, actorNNHidden, "ActorStateEncoder");
            encodedVectorStateCritic = CreateContinuousStateEncoder(inVectorstate, criticNNWidth, criticNNHidden, "CriticStateEncoder");
        }

        //concat all inputs
        Tensor encodedAllActor = null;
        Tensor encodedAllCritic = null;

        if(inVisualState == null && inVectorstate != null)
        {
            encodedAllActor = encodedVectorStateActor;
            encodedAllCritic = encodedVectorStateCritic;
        }
        else if(inVisualState != null && inVectorstate == null)
        {
            encodedAllActor = encodedVisualActor;
            encodedAllCritic = encodedVisualCritic;
        }
        else if(inVisualState != null && inVectorstate != null)
        {
            encodedAllActor = Current.K.concat(new List<Tensor>() { encodedVectorStateActor,encodedVisualActor},1);
            encodedAllCritic = Current.K.concat(new List<Tensor>() { encodedVectorStateCritic, encodedVisualCritic }, 1);
        }


        //outputs
        var actorOutput = new Dense(units: outActionSize, activation: null, use_bias: true, kernel_initializer: new GlorotUniform(scale: outputWeightsInitialScale));
        outAction = actorOutput.Call(encodedAllActor)[0];
        weights.AddRange(actorOutput.weights);

        var criticOutput = new Dense(units: 1, activation: null, use_bias: true, kernel_initializer: new GlorotUniform(scale: outputWeightsInitialScale));
        outValue = criticOutput.Call(encodedAllCritic)[0];
        weights.AddRange(criticOutput.weights);
    }


    protected Tensor CreateVisualEncoder(Tensor visualInput, int hiddenSize, int numLayers, string scope)
    {
        //use the same encoder as in UnityML's python codes
        Tensor temp;
        using (Current.K.name_scope(scope))
        {
            var conv1 = new Conv2D(16, new int[] { 8, 8 }, new int[] { 4, 4 }, activation: new ELU());
            var conv2 = new Conv2D(32, new int[] { 4, 4 }, new int[] { 2, 2 }, activation: new ELU());

            temp = conv1.Call(visualInput)[0];
            temp = conv2.Call(temp)[0];

            temp = Current.K.batch_flatten(temp);

            weights.AddRange(conv1.weights);
            weights.AddRange(conv2.weights);
        }

        var hiddenFlat = CreateContinuousStateEncoder(temp, hiddenSize, numLayers, scope);
        return hiddenFlat;
    }

    protected Tensor CreateContinuousStateEncoder(Tensor state, int hiddenSize, int numLayers, string scope)
    {
        var hidden = state;
        using (Current.K.name_scope(scope))
        {
            for (int i = 0; i < numLayers; ++i)
            {
                var layer = new Dense(hiddenSize, new ReLU(), true, kernel_initializer: new GlorotUniform(scale: hiddenWeightsInitialScale));
                hidden = layer.Call(hidden)[0];
                weights.AddRange(layer.weights);
            }
        }

        return hidden;
    }


    public override List<Tensor> GetWeights()
    {
        return weights;
    }
}