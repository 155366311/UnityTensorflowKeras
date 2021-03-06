# Training with Behaviour Cloning(Supervised Learning)

This algorithm is basically trying to train the neural network to remember what the correct action is in different states. See [Unity's document](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Training-Imitation-Learning.md) for more explanation.

The example scene `UnityTensorflow/Examples/Pong/PongSL` shows how to use supervised learning to train the neural network from how you are playing the game yourself. 

## Overall Steps to Setup
1. Create a environment using ML-Agent API. See the [instruction from Unity](https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Learning-Environment-Create-New.md). When creae the brain scripable object, create an Internal Learning Brain `(Create/ML-Agents/InternalLearningBrain)` instead of others and set the correct brain parameters.
2. Create a Trainer
	1. Attach a `TrainerMimic.cs` to any GameObject.
    2. Create a `TrainerParamsMimic` scriptable object with proper parameters in your project(in project window selelct `Create/ml-agent/ppo/TrainerParamsMimic`), and assign it to the Params field in `TrainerMimic.cs`.
    3. Assign the Internal Learning Brain created previously to the brainToTrain field in `TrainerMimic.cs`.
3. Create a Model
	1. Attach a `SupervisedLearningModel.cs` to any GameObject.
    2. Create a `SupervisedLearningNetworkSimple` scriptable object in your project(in project window selelct `Create/ml-agent/ppo/SupervisedLearningNetworkSimple`), and assign it to the Network field in `SupervisedLearningModel.cs`.
    3. Assign the created Model to the `modelRef` field of in `TrainerMimic.cs`
    
4. Create a Decision
  1. You can either use PlayerDecision.cs directly if you want the neural network to learn from human playing the game, or inherit from [AgentDependentDecision](AgentDependentDeicision.md) if you want the agent to learn from other scripted AI.
  2. Attach the decision script to the agent that you want to learn from and check the `useDecision` in inspector.

5. Play! But some notes:
  * The trainer only collect data from agents with Decision attached to it.
  * Only when enough data is collected, it will start training(set the value in trainer parameters)
  * The `isCollectinData` field in trainer needs to be true to collect training data. 
  
## Explanation of fields in the inspector
#### TrainerMimic.cs
* `isTraining`: Toggle this to switch between training and inference mode. Note that if isTraining if false when the game starts, the training part of the PPO model will not be initialize and you won't be able to train it in this run. Also,
* `parameters`: You need to assign this field with a TrainerParamsMimic scriptable object. 
* `continueFromCheckpoint`: If true, when the game starts, the trainer will try to load the saved checkpoint file to resume previous training.
* `checkpointPath`:  The path of the checkpoint directory. 
* `checkpointFileName`: The name of the checkpoint file 
* `steps`: Just to show you the current step of the training.
* `isCollectingData`: If the training is collecting training data from Agents with Decision.
* `trainingDataSaveFileName`:  The name of the training data file. The collected training data is saved/loaded from here. 
* `dataBufferCount`: Shows the current collected data count.

#### TrainerParamsMimic
* `learningRate`: Learning rate used to train the neural network.
* `maxTotalSteps`: Max steps the trainer will be training.
* `saveModelInterval`: The trained model will be saved every this amount of steps.
* `logInterval`: How many traing steps between each logging.
* `batchSize`: Mini batch size when training.
* `numIterationPerTrain`: How many batches to train for each step(fixed update).
* `requiredDataBeforeTraining`: How many collected data count is needed before it start to traing the neural network.
* `maxBufferSize`: Max buffer size of collected data. If the data buffer count exceeds this number, old data will be overrided. Set this to 0 to remove the limit.

#### SupervisedLearningModel.cs
* `checkpointToLoad`: If you assign a model's saved checkpoint file to it, this will be loaded when model is initialized, regardless of the trainer's loading. Might be used when you are not using a trainer.
* `modelName`: The name of the model. It is used for the namescope When buliding the neural network. Can be empty by default.
* `weightSaveMode`: This decides the names of the weights of neural network when saving to checkpoints as serialized dictionary. No need to changes this ususally. 
* `Network`: You need to assign this field with a scriptable object that implements RLNetworkPPO.cs. 
* `optimizer`: The optimizer to use for this model when training. You can also set its parameters here.

#### SupervisedLearningNetworkSimple
This is a simple implementation of SuperviseLearningNetowrk that you can create a plug it in as a neural network definition for any SupervisedLearningModel.
- `hiddenLayers`: Hidden layers of the network. The array size if the number of hidden layers. In each element, there are for parameters that defines each layer. Those do not have default values, so you have to fill them.
	- size: Size of this hidden layer. 
    - initialScale: Initial scale of the weights. This might be important for training.Try something larger than 0 and smaller than 1.
    - useBias: Whether Use bias. Usually true.
    - activationFunction: Which activation function to use. Usually Relu.
- `outputLayerInitialScale`/`visualEncoderInitialScale`: Initial scale of the weights of the output layers.
- `outputLayerBias`/`visualEncoderBias`: Whether use bias.
- `useVarianceForCoutinuousAction`: Whether also output a variance of the action if the action space is continuous.
- `minStd`: If it does outputs a variance of the action, the standard deviation will always be larger than this value.

## Training using GAN
You can also use a [conditional GAN](https://arxiv.org/abs/1411.1784) model instead of regular supervised learning model. GAN might be better if the correct actions of the same observation do not follow guassian distribution. However, training of GAN is very unstable.

Note that currently the GAN network we made does not support visual observation.

#### Steps to Setup
Most the same steps as using regular [supervised learning](Overall Steps) as before, but change step 3 to create a GAN model, and change the `TrainerParamsMimic` in step 2-2 to `TrainerParamsGAN` instead.

- Create a GAN model:
	1. Attach a `GANModel.cs` to any GameObject.
    2. Create a `GANNetworkDense` scriptable object in your project and assign it to the Network field in `GANModel.cs`.
    3. Assign the created Model to the `modelRef` field of in `TrainerMimic.cs`
    
For the details of GAN's parameters, check [Training a Generative Adversarial Network(GAN)](Training-GAN.md) for details.

## Create your own neural network architecture
If you want to have your own neural network architecture instead of the one provided by [`SupervisedLearningNetworkSimple`](#supervisedlearningnetworksimple), you can inherit `SupervisedLearningNetwork` class to build your own neural network. See the [sourcecode](https://github.com/tcmxx/UnityTensorflowKeras/blob/tcmxx/docs/Assets/UnityTensorflow/Learning/Mimic/SupervisedLearningNetwork.cs) of `SupervisedLearningNetwork.cs` for documentation.
